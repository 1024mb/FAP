﻿#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Foundation;
using Fap.Network.Services;
using Autofac;
using System.Net;
using Fap.Network.Entity;
using Fap.Network;
using System.Threading;
using Fap.Domain.Verbs;
using System.Net.Sockets;
using Fap.Foundation.Logging;
using System.Xml.Linq;
using System.Xml;
using Fap.Domain.Entity;

namespace Fap.Domain.Controllers
{
    public class OverlordController : AsyncControllerBase
    {
        private bool running = false;
        private FAPListener listener;
        private IContainer container;
        private BroadcastServer bserver;
        private BroadcastClient bclient;
        private ConnectionService connectionService;
        private BufferService bufferService;
        private string listenLocation;
        private Logger logService;
        
        //Announcer
        private Thread announcer;
        private long lastAnnounce;
        private long lastRequest;
        private readonly long minAnnounceFreq = 10000;
        private readonly long maxAnnounceFreq = 500;

        private string networkID;
        private string networkName;
        private Model model;
        private Overlord omodel;

        public OverlordController(IContainer c)
        {
            container = c;
            logService = c.Resolve<Logger>();
            model = c.Resolve<Model>();
            omodel = model.Overlord;
            model.Overlord.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Overlord_PropertyChanged);
        }

        void Overlord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MaxPeers":
                    GenerateStrength();
                    break;
            }
        }

        public void Start(IPAddress ip, int port, string id, string name)
        {
            if (!running)
            {
                networkID = id;
                networkName = name;
                listener = container.Resolve<FAPListener>();
                listener.OnReceiveRequest += new FAPListener.ReceiveRequest(listener_OnReceiveRequest);
                port = listener.Start(ip, port);
                listenLocation = ip.ToString() + ":" + port;
                bserver = container.Resolve<BroadcastServer>();
                bclient = container.Resolve<BroadcastClient>();
                bufferService = container.Resolve<BufferService>();
                connectionService = container.Resolve<ConnectionService>();
                bclient.OnBroadcastCommandRx += new BroadcastClient.BroadcastCommandRx(bclient_OnBroadcastCommandRx);
                ThreadPool.QueueUserWorkItem(new WaitCallback(Process_announce));
                model.Overlord.Host = ip.ToString();
                model.Overlord.Port = port;
                model.Overlord.Nickname = "Overlord";
                bclient.StartListener();
            }
            else
                throw new Exception("Super node alrady running.");
        }

        public void Stop()
        {
            announcer.Abort();
            listener.Stop();
            DisconnectVerb verb = new DisconnectVerb(model.Overlord);
            TransmitToAll(verb.CreateRequest());

            int startTime = Environment.TickCount;

            //Wait for outstanding streams to empty for up to 4 seconds.
            while (omodel.Peers.Where(p => p.Running).Select(p => p.PendingRequests).Sum() > 0 && Environment.TickCount-startTime<4000)
                Thread.Sleep(25);
        }
        


        private void GenerateStrength()
        {
            Random r = new Random(Environment.TickCount);

            switch (model.Overlord.MaxPeers)
            {
                case OverlordLimits.HIGH_PRIORITY:
                    model.Overlord.Strength = r.Next(666, 1000);
                    break;
                case OverlordLimits.LOW_PRIORITY:
                    model.Overlord.Strength = r.Next(0, 333);
                    break;
                default:
                    model.Overlord.Strength = r.Next(333, 666);
                    break;
            }

        }

        private void TransmitToAll(Request r)
        {
            foreach (var peer in omodel.Peers.ToList())
                peer.AddMessage(r);
        }

        /// <summary>
        /// Broadcast RX
        /// </summary>
        /// <param name="cmd"></param>
        private void bclient_OnBroadcastCommandRx(Request cmd)
        {
            //logService.AddInfo("Overlord rx: " + cmd.Command + " P: " + cmd.Param);
            switch (cmd.Command)
            {
                case "HELLO":
                    HandleHello(cmd);
                    break;
                case "WHO":
                    lock (announcer)
                        lastRequest = Environment.TickCount;
                    break;
            }
        }

        /// <summary>
        /// Unicast RX
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool listener_OnReceiveRequest(Request r, Socket s)
        {
            switch (r.Command)
            {
                case "CONNECT":
                    return HandleConnect(r, s);
                case "CLIENT":
                    return HandleClient(r, s);
                case "CHAT":
                    return HandleChat(r, s);
                case "DISCONNECT":
                    return HandleDisconnect(r, s);
                case "INFO":
                    return HandleInfo(r, s);
                /* default:
                     VerbFactory factory = new VerbFactory();
                     var verb = factory.GetVerb(r.Command, model);
                     s.Send(Mediator.Serialize(verb.ProcessRequest(r)));
                     return false;*/
            }

            return false;
        }



        private bool HandleInfo(Request r, Socket s)
        {
            InfoVerb info = new InfoVerb(omodel);
            s.Send(Mediator.Serialize(info.ProcessRequest(r)));
            return false;
        }

        /// <summary>
        /// Incoming disconnect from a peer, remove and alert other peers
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool HandleDisconnect(Request r, Socket s)
        {
            var search = model.Overlord.Peers.Where(p => p.Node.Secret == r.RequestID && p.Node.ID == r.Param).FirstOrDefault();
            Response response = new Response();
            response.RequestID = r.RequestID;

            if (null != search)
            {
                omodel.Peers.Remove(search);
                search.OnDisconnect -= new PeerStream.Disconnect(peer_OnDisconnect);
                TransmitToAll(r);
                response.Status = 0;
            }
            else
            {
                response.Status = 1;
            }
            s.Send(Mediator.Serialize(response));
            return false;
        }

       /// <summary>
       /// Receive a chat message, if from a valid peer then forward onto all other peers
       /// </summary>
       /// <param name="r"></param>
       /// <param name="s"></param>
       /// <returns></returns>
        private bool HandleChat(Request r, Socket s)
        {
            var search = omodel.Peers.Where(p => p.Node.Secret == r.RequestID && p.Node.ID == r.Param).FirstOrDefault();
            if (null != search)
            {
                TransmitToAll(r);
                Response response = new Response();
                response.RequestID = r.RequestID;
                response.Status = 0;
                s.Send(Mediator.Serialize(response));
            }
            else
            {
                Response response = new Response();
                response.RequestID = r.RequestID;
                response.Status = 1;
                s.Send(Mediator.Serialize(response));
            }
            return false;
        }

        /// <summary>
        /// Receive a broadcast hello request.  These should only be received from other overlord peers.
        /// </summary>
        /// <param name="cmd"></param>
        private void HandleHello(Request cmd)
        {
            //Check we don't know about the peer already.
            HelloVerb verb = new HelloVerb(model.Overlord);
            verb.ProcessRequest(cmd);
            var search = omodel.Peers.Where(p => p.Node.ID == verb.ID).FirstOrDefault();
            if (null != search)
                return;
            if (omodel.ID == verb.ID)
                return;
            //Connect remote client async as it may take time to fail.
            ThreadPool.QueueUserWorkItem(HandleHeloAsync, cmd);
        }

        private void HandleHeloAsync(object o)
        {
            try
            {
                Request cmd = o as Request;
                if (null != cmd)
                {
                    Node n = new Node();
                    n.Location  =cmd.Param;
                    
                    var session = connectionService.GetClientSession(n);
                    Client client = new Client(bufferService, connectionService);
                    InfoVerb info = new InfoVerb(model.Overlord);

                    if (client.Execute(info, n))
                    {
                        var search = omodel.Peers.Where(p => p.Node.ID == n.ID).FirstOrDefault();
                        if (null == search)
                        {
                            PeerStream peer = new PeerStream(n, session);
                            peer.OnDisconnect += new PeerStream.Disconnect(peer_OnDisconnect);
                            omodel.Peers.Add(peer);
                        }
                    }
                }
            }
            catch { }
        }

        private void peer_OnDisconnect(PeerStream s)
        {
            if (omodel.Peers.Contains(s))
                omodel.Peers.Remove(s);
            //Try to send disconnect notification
            try
            {
                DisconnectVerb verb = new DisconnectVerb(omodel);
                var session = connectionService.GetClientSession(s.Node);
                var req = verb.CreateRequest();
                req.RequestID = s.Node.Secret;
                session.Socket.Send(Mediator.Serialize(req));
            }
            catch
            {

            }
            s.OnDisconnect -= new PeerStream.Disconnect(peer_OnDisconnect);
        }

        /// <summary>
        /// Handles announcing presence via broadcast.
        /// </summary>
        private void Process_announce(object o)
        {
            announcer = Thread.CurrentThread;
            int sleepTime = 25;
            while (true)
            {
                bool doAnnounce = false;
                lock (announcer)
                {
                    bool reqAnnounce = lastRequest != 0;
                    bool timerExpired = lastAnnounce + minAnnounceFreq < Environment.TickCount;
                    bool allowAnnounce = lastAnnounce + maxAnnounceFreq < Environment.TickCount;

                    if ((reqAnnounce || timerExpired) && allowAnnounce)
                    {
                        doAnnounce = true;
                        lastRequest = 0;
                    }
                }
                if (doAnnounce)
                {
                    lastAnnounce = Environment.TickCount;
                    HelloVerb helo = new HelloVerb(model.Overlord);
                    helo.ListenLocation = listenLocation;
                    bserver.SendCommand(helo.CreateRequest());
                }
                Thread.Sleep(sleepTime);
            }
        }

        /// <summary>
        /// Receive a request containing updated client record information, if a valid client store and forward on.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool HandleClient(Request r, Socket s)
        {
            var client = omodel.Peers.Where(p => p.Node.ID == r.Param && r.RequestID == p.Node.Secret).FirstOrDefault();
            if (null == client)
            {
                //Unregisted client or invalid info.
                Response response = new Response();
                response.RequestID = r.RequestID;
                response.Status = 1;
                s.Send(Mediator.Serialize(response));
                return false;
            }
            //Client is ok, replicate new info.
            if (r.AdditionalHeaders.Count > 0)
            {
                foreach (var info in r.AdditionalHeaders)
                    client.Node.SetData(info.Key, info.Value);
                TransmitToAll(r);
                Response response = new Response();
                response.RequestID = r.RequestID;
                response.Status = 0;
                s.Send(Mediator.Serialize(response));
            }
            return false;
        }

        /// <summary>
        /// Receive a logon request from node 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool HandleConnect(Request r, Socket s)
        {
            Response response = new Response();
            Client c = new Client(bufferService, connectionService);
            Node clientNode = new Node();
            InfoVerb info = new InfoVerb(clientNode);

            clientNode.Location = r.Param;
            clientNode.Secret = r.RequestID;
            if (c.Execute(info, clientNode))
            {
                if (info.Status == 0)
                {
                    bool reconnect = false;
                    var search = omodel.Peers.ToList().Where(p => p.Node.ID == clientNode.ID).FirstOrDefault();
                    //Remove old stream if there is one
                    if (search != null)
                    {
                        search.Kill();
                        omodel.Peers.Remove(search);
                        search.OnDisconnect -= new PeerStream.Disconnect(peer_OnDisconnect);
                        reconnect = true;
                    }

                    search = new PeerStream(clientNode, connectionService.GetClientSession(clientNode));
                    omodel.Peers.Add(search);
                   
                    response.Status = 0;//ok
                  
                    //Transmit client info to other clients

                    //If the client is reconnecting then clear out old info by sending a disconnect first.
                    if (reconnect)
                    {
                        DisconnectVerb disconnect = new DisconnectVerb(clientNode);
                        TransmitToAll(disconnect.CreateRequest());
                    }

                    ClientVerb verb = new ClientVerb(clientNode, "");
                    TransmitToAll(verb.CreateRequest());


                    //Transmit all known clients to the connecting node
                    verb = new ClientVerb(omodel, "");
                    search.AddMessage(verb.CreateRequest());

                    foreach (var client in omodel.Peers.ToList())
                    {
                        verb = new ClientVerb(client.Node, "");
                        search.AddMessage(verb.CreateRequest());
                    }
                }
                else
                {
                    response.Status = 3;//Other error
                }

            }
            else
            {
                response.Status = 2;//Could not connect
            }
            response.AdditionalHeaders.Add("Host", model.Overlord.ID);
            response.AdditionalHeaders.Add("ID", networkID);
            response.AdditionalHeaders.Add("Name", networkName);
            response.RequestID = r.RequestID;
            s.Send(Mediator.Serialize(response));
            return false;
        }

        /// <summary>
        /// Scan the client machine for services such as HTTP or samba shares
        /// </summary>
        /// <param name="n"></param>
        private void ScanClient(Node n)
        {
            //Check for HTTP
            string webTitle = string.Empty;
            try
            {
                WebClient wc = new WebClient();
                string html = wc.DownloadString("http://" + n.Host);
                if (!string.IsNullOrEmpty(html))
                    webTitle = "Web";
                XDocument doc = XDocument.Parse(html);
                var title = doc.Elements("html").Where(x => (string.Equals(x.Name.LocalName, "title", StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();
                if (null != title && !string.IsNullOrEmpty(title.Value))
                    webTitle = title.Value;
            }
            catch { }

            //Check for FTP
            string ftp = string.Empty;
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(n.Host, 21);
                ftp = "FTP";
                StringBuilder sb = new StringBuilder();
                long start = Environment.TickCount+3000;
                byte[] data = new byte[20000];
                client.ReceiveBufferSize = data.Length;

                while (start > Environment.TickCount && client.Connected)
                {
                    if (client.GetStream().DataAvailable)
                    {
                        int length = client.GetStream().Read(data, 0, data.Length);
                        sb.Append(Encoding.ASCII.GetString(data, 0, length));
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
                client.Close();

                string title = sb.ToString();
                if (!string.IsNullOrEmpty(title))
                    ftp = title;
            }
            catch { }

            //Check for samba shares

            string samba = string.Empty;
            try
            {
                var shares = ShareCollection.GetShares(n.Host);
                StringBuilder sb = new StringBuilder();
                foreach (SambaShare share in shares)
                {
                    if (share.IsFileSystem && share.ShareType == ShareType.Disk)
                    {
                        try
                        {
                            if(sb.Length>0)
                                sb.Append("|");
                            //Make sure its readable
                            System.IO.DirectoryInfo[] Flds = share.Root.GetDirectories();
                            sb.Append(share.NetName);
                           
                        }
                        catch { }
                    }
                }
                samba = sb.ToString();
            }
            catch { }


            //Update client
            if (n.GetData("HTTP") != webTitle ||
               n.GetData("FTP") != ftp ||
               n.GetData("Shares") != samba)
            {
                n.SetData("HTTP", webTitle);
                n.SetData("FTP", ftp);
                n.SetData("Shares", samba);
                //Fake a update request
                Request r = new Request();
                r.Command = "CLIENT";
                r.Param = n.ID;
                r.AdditionalHeaders.Add("HTTP", webTitle);
                r.AdditionalHeaders.Add("FTP", ftp);
                r.AdditionalHeaders.Add("Shares", samba);
               TransmitToAll(r);
            }
        }
    }
}