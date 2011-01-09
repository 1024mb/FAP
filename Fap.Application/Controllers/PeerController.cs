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
using Fap.Domain.Services;
using Fap.Domain.Entity;
using Fap.Application.ViewModels;
using Fap.Domain;
using Fap.Domain.Commands;
using System.Windows.Threading;
using System.Threading;
using Fap.Network.Services;
using Fap.Foundation.Logging;
using Fap.Network.Entity;
using Fap.Network;
using Autofac;
using Fap.Domain.Verbs;
using Fap.Domain.Controllers;
using System.Net;
using System.Net.Sockets;
using Fap.Foundation.Services;
using Fap.Foundation;

namespace Fap.Application.Controllers
{
    /// <summary>
    /// Manages node to overlord communications,
    /// </summary>
    public class PeerController
    {
        private BroadcastClient client;
        private BroadcastServer server;

        private IContainer container;
        private object sync = new object();

        //Models
        private SafeObservable<DetectedOverlord> overlordList = new SafeObservable<DetectedOverlord>();
        private Model model;
        private Node transmitted = new Node();
        private Fap.Domain.Entity.Network network;
        private long lastConnected = Environment.TickCount;

        //Workers
        private Thread syncworker;
        private Thread connectionHandler;

        //Services
        private ConnectionService connectionService;
        private BufferService bufferService;
        private OverlordController overlord;
        private Logger logger;

        private readonly long OVERLORD_LASTSEEN_TIMEOUT = 60000;//ms



        public bool IsOverlord
        {
            get { return null != overlord; }
        }

        public PeerController(IContainer c)
        {
            container = c;
            client = container.Resolve<BroadcastClient>();
            client.OnBroadcastCommandRx += new BroadcastClient.BroadcastCommandRx(client_OnBroadcastCommandRx);
            connectionService = container.Resolve<ConnectionService>();
            logger = container.Resolve<Logger>();
            model = container.Resolve<Model>();
            server = container.Resolve<BroadcastServer>();
            bufferService = container.Resolve<BufferService>();
        }

        public void Start(Fap.Domain.Entity.Network local)
        {
            network = local;
            local.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(local_PropertyChanged);
            logger.AddInfo("Attempting to connect to the local FAP network..");
            client.StartListener();
            //Find current servers
            ThreadPool.QueueUserWorkItem(new WaitCallback(SendWhoAsync));

            ThreadPool.QueueUserWorkItem(new WaitCallback(ConnectionHandler));
            ThreadPool.QueueUserWorkItem(new WaitCallback(SyncWorker));
        }

        private void local_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Reset the start overlord timer each time the network status changes so on disconnect we dont just start a overlord straight away
            lastConnected = Environment.TickCount;
        }

        private void ConnectionHandler(object ox)
        {
            int sleep = 500;
            while (true)
            {
                try
                {
                    if (network.State != ConnectionState.Connected)
                    {
                        if (network.State != ConnectionState.Connecting)
                            network.State = ConnectionState.Connecting;

                        //Copy list of servers
                        List<DetectedOverlord> servers = overlordList.ToList();
                        servers = servers.Where(o => o.Clients < 50).OrderBy(o => o.Clients).ToList();

                        //If no overlords after a time out then start our own.
                        if (servers.Count == 0 && (Environment.TickCount - lastConnected) > 2000 && null == overlord)
                        {
                            logger.AddInfo("No valid overlord detected after 2 seconds, starting a local overlord.");
                            overlord = container.Resolve<OverlordController>();
                            overlord.Start(GetLocalAddress(), 90, "LOCAL", "Local");
                            Thread.Sleep(15);
                            continue;
                        }

                        foreach (var server in servers)
                        {
                            try
                            {
                                ConnectVerb connect = new ConnectVerb(model.Node);
                                connect.RemoteLocation = model.Node.Location;

                                Client c = container.Resolve<Client>();

                                Node serverNode = new Node();
                                serverNode.Location = server.Location;

                                network.Secret = IDService.CreateID();

                                if (c.Execute(connect, serverNode, network.Secret))
                                {
                                    if (connect.Status == 0)
                                    {
                                        if (string.IsNullOrEmpty(connect.OverlordID) || string.IsNullOrEmpty(connect.NetworkID))
                                        {
                                            //We didnt get back valid network info so try another server.
                                            logger.AddWarning("Connect failed to return valid network info.");
                                            continue;
                                        }
                                        var search = model.Networks.Where(n => n.ID == connect.NetworkID).FirstOrDefault();
                                        if (null == search)
                                        {
                                            search = new Domain.Entity.Network();
                                            search.ID = "LOCAL";// connect.NetworkID;
                                            model.Networks.Add(search);
                                        }
                                        search.Name = connect.Name;
                                        search.OverlordID = connect.OverlordID;
                                        search.Secret = connect.Secret;
                                        search.State = ConnectionState.Connected;
                                        logger.AddInfo("Connected to the local FAP network.");
                                        break;
                                    }
                                }
                            }
                            catch
                            {
                                overlordList.Lock();
                                if (overlordList.Contains(server))
                                    overlordList.Remove(server);
                                overlordList.Unlock();
                            }
                        }
                    }
                }
                catch { }
                Thread.Sleep(sleep);
            }
        }

        private void SyncWorker(object ox)
        {
            int sleep = 500;
            while (true)
            {
                Thread.Sleep(sleep);
                //Send model changes if connected
                if (null != network && network.State == ConnectionState.Connected)
                    CheckModelChanges();
                //Remove overlords we haven't seen for a bit
                overlordList.Lock();
                foreach (var overlord in overlordList.Where(o => o.LastSeen + OVERLORD_LASTSEEN_TIMEOUT < Environment.TickCount).ToList())
                    overlordList.Remove(overlord);
                overlordList.Unlock();
            }
        }


        private void client_OnBroadcastCommandRx(Request cmd)
        {
            switch (cmd.Command)
            {
                case "HELO":
                    HandleHelo(cmd);
                    break;
                case "WHO":
                    //Do nothing
                    break;
            }
        }

        private void HandleHelo(Request r)
        {
            HeloVerb hello = new HeloVerb(null);
            hello.ProcessRequest(r);
            lock (sync)
            {
                var search = overlordList.Where(o => o.Location == hello.ListenLocation).FirstOrDefault();
                if (null == search)
                {
                    overlordList.Add(new DetectedOverlord()
                    {
                        Clients = hello.Clients,
                        Index = hello.Index,
                        LastSeen = Environment.TickCount,
                        Location = hello.ListenLocation
                    });
                }
                else
                {
                    search.Clients = hello.Clients;
                    search.Index = hello.Index;
                    search.LastSeen = Environment.TickCount;
                    search.Location = hello.ListenLocation;
                }
            }
        }


        private IPAddress GetLocalAddress()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress a = localIPs[0];

            foreach (var ip in localIPs)
            {
                if (!IPAddress.IsLoopback(ip) && ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    a = ip;
                    break;
                }
            }
            return a;
        }




        private void SendWhoAsync(object o)
        {
            WhoVerb verb = new WhoVerb();
            server.SendCommand(verb.CreateRequest());
        }

        /// <summary>
        /// Whilst connected to a network 
        /// </summary>
        private void CheckModelChanges()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (var entry in model.Node.Data)
            {
                if (transmitted.IsKeySet(entry.Key))
                {
                    if (transmitted.GetData(entry.Key) != entry.Value)
                    {
                        data.Add(entry.Key, entry.Value);
                    }
                }
                else
                {
                    data.Add(entry.Key, entry.Value);
                }
            }

            //Data has changed, transmit the changes.
            if (data.Count > 0)
            {
                var network = model.Networks.Where(n => n.ID == "LOCAL").FirstOrDefault();
                if (null != network)
                {
                    var node = model.Peers.Where(p => p.ID == network.OverlordID).FirstOrDefault();
                    if (null != node)
                    {
                        Request request = new Request();
                        request.RequestID = network.Secret;
                        request.Command = "CLIENT";
                        request.Param = model.Node.ID;
                        foreach (var change in data)
                        {
                            request.AdditionalHeaders.Add(change.Key, change.Value);
                            transmitted.SetData(change.Key, change.Value);
                        }

                        Client client = new Client(bufferService, connectionService);
                        Response response = null;
                        if (!client.Execute(request, node, out response) || response.Status != 0)
                        {
                            network.State = ConnectionState.Disconnected;
                        }
                    }
                }
            }
        }



        public void SendChatMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendChatMessageAsync), message);
        }


        private void SendChatMessageAsync(object o)
        {
            string message = o as string;
            string secret = string.Empty;
            Session overlord = GetOverlordConnection(out secret);
            if (null != overlord)
            {
                Request r = new Request();
                r.RequestID = secret;
                r.Command = "CHAT";
                r.Param = model.Node.ID;
                r.AdditionalHeaders.Add("Text", message);
                r.AdditionalHeaders.Add("Name", model.Node.Nickname);

                Client c = new Client(bufferService, connectionService);
                Response response = new Response();
                if (!c.Execute(r, overlord, out response) || response.Status != 0)
                {
                    logger.AddError("Failed to send chat message, try again shortly.");
                }
            }
            else
            {
                logger.AddError("Failed to send chat message, you are currently not connected.");
            }
        }




        public Session GetOverlordConnection(out string secret)
        {
            if (network.State == ConnectionState.Connected)
            {
                var overlord = model.Peers.Where(n => n.ID == network.OverlordID).FirstOrDefault();
                if (null != overlord)
                {
                    secret = network.Secret;
                    return connectionService.GetClientSession(overlord);
                }
            }
            secret = null;
            return null;
        }


        public class DetectedOverlord
        {
            public string Location { set; get; }
            public int Clients { set; get; }
            public long LastSeen { set; get; }
            public int Index { set; get; }
        }
    }
}