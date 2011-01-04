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
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using System.Net;
using Fap.Network.Entity;

namespace Fap.Domain.Entity
{
    [Serializable]
    public class Model : INotifyPropertyChanged
    {
        public readonly string SoftwareVersion = "FAP 2.0";
        [XmlIgnore]
        public static readonly int ClientVersion = 0;
        [XmlIgnore]
        public static readonly int NetCodeVersion = 3;

        [XmlIgnore]
        private readonly string saveLocation;
        [XmlIgnore]
        private DownloadQueue downloadQueue;
        private int maxDownloads;
        private int maxDownloadsPerUser;
        private int maxUploads;
        private int maxUploadsPerUser;
        private string downloadFolder;


        [XmlIgnore]
        private SafeObservable<Session> sessions;
        [XmlIgnore]
        private Node node;
        [XmlIgnore]
        private SafeObservable<Node> peers;
        [XmlIgnore]
        private SafeObservable<Share> shares;
        [XmlIgnore]
        private SafeObservable<Network> networks;
        [XmlIgnore]
        private SafeObservable<string> messages;  

        public Model()
        {
            peers = new SafeObservable<Node>();
            shares = new SafeObservable<Share>();
            networks = new SafeObservable<Network>();
            node = new Fap.Network.Entity.Node();
            sessions = new SafeObservable<Session>();
            messages = new SafeObservable<string>();
            saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FAP\Config.xml";
            node.PropertyChanged += new PropertyChangedEventHandler(node_PropertyChanged);
        }

        void node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyChange(e.PropertyName);
        }

        public SafeObservable<Share> Shares
        {
            set { shares = value; NotifyChange("Shares"); }
            get { return shares; }
        }

        [XmlIgnore]
        public SafeObservable<string> Messages
        {
            set { messages = value; NotifyChange("Messages"); }
            get { return messages; }
        }

        [XmlIgnore]
        public SafeObservable<Network> Networks
        {
            set { networks = value; NotifyChange("Networks"); }
            get { return networks; }
        }

        [XmlIgnore]
        public SafeObservable<Node> Peers
        {
            set { peers = value; NotifyChange("Peers"); }
            get { return peers; }
        }

        [XmlIgnore]
        public Node Node
        {
            set { node = value; NotifyChange("Node"); }
            get { return node; }
        }

        [XmlIgnore]
        public SafeObservable<Session> Sessions
        {
            get { return sessions; }
        }
        
        //Properties
        [XmlIgnore]
        public DownloadQueue DownloadQueue
        {
            get { return downloadQueue; }
            set { downloadQueue = value; }
        }


        public string DownloadFolder
        {
            set
            {
                downloadFolder = value;
                NotifyChange("DownloadFolder");
            }
            get
            {
                return downloadFolder;
            }
        }

        public string Nickname
        {
            set
            {
                node.Nickname = value;
            }
            get
            {
                return node.Nickname;
            }
        }

        public string Description
        {
            set
            {
                node.Description = value;
            }
            get
            {
                return node.Description;
            }
        }

        public int MaxDownloads
        {
            set
            {
                maxDownloads = value;
                NotifyChange("MaxDownloads");
            }
            get
            {
                return maxDownloads;
            }
        }

        public int MaxDownloadsPerUser
        {
            set
            {
                maxDownloadsPerUser = value;
                NotifyChange("MaxDownloadsPerUser");
            }
            get
            {
                return maxDownloadsPerUser;
            }
        }

        public int MaxUploads
        {
            set
            {
                maxUploads = value;
                NotifyChange("MaxUploads");
            }
            get
            {
                return maxUploads;
            }
        }


        public int MaxUploadsPerUser
        {
            set
            {
                maxUploadsPerUser = value;
                NotifyChange("maxUploadsPerUser");
            }
            get
            {
                return maxUploadsPerUser;
            }
        }

        public string AvatarBase64
        {
            set
            {
                node.AvatarBase64 = value;
            }
            get
            {
                return node.AvatarBase64;
            }
        }

        [XmlIgnore]
        public byte[] Avatar
        {
            set
            {
                node.Avatar = value;
            }
            get
            {
                return node.Avatar;
            }
        }

        public long TotalShareSize
        {
            get
            {
                long total = 0;
                lock (Shares)
                {
                    foreach (var share in Shares.ToList())
                    {
                        total += share.Size;
                    }
                }
                return total;
            }
        }

        [XmlIgnore]
        public string TotalShareSizeString
        {
            get { return Utility.FormatBytes(TotalShareSize); }
        }

        public void Save()
        {
            if (!Directory.Exists(Path.GetDirectoryName(saveLocation)))
                Directory.CreateDirectory(Path.GetDirectoryName(saveLocation));

            XmlSerializer serializer = new XmlSerializer(typeof(Model));
            using (TextWriter textWriter = new StreamWriter(saveLocation))
            {
                serializer.Serialize(textWriter, this);
                textWriter.Flush();
                textWriter.Close();
            }
        }

        public void Load()
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(Model));
                using (TextReader textReader = new StreamReader(saveLocation))
                {
                    Model m = (Model)deserializer.Deserialize(textReader);
                    textReader.Close();
                    Shares = m.Shares;
                    AvatarBase64 = m.AvatarBase64;
                    Description = m.Description;
                    Nickname = m.Nickname;
                    DownloadFolder = m.DownloadFolder;
                    MaxDownloads = m.MaxDownloads;
                    MaxDownloadsPerUser = m.MaxDownloadsPerUser;
                    MaxUploads = m.MaxUploads;
                    MaxUploadsPerUser = m.MaxUploadsPerUser;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to read config", e);
            }
        }

        private void NotifyChange(string path)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(path));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}