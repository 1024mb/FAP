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
using System.Waf.Applications;
using Fap.Application.Views;
using Fap.Domain.Entity;
using Fap.Foundation;
using System.Windows.Input;
using System.Windows.Threading;
using Fap.Network.Entity;

namespace Fap.Application.ViewModels
{
    public class MainWindowViewModel : ViewModel<IMainWindow>
    {
        private SafeObservable<string> chatList = new SafeObservable<string>();
        private SafeObservable<Session> sessions = new SafeObservable<Session>();

        private string currentChatMessage;
        private ICommand sendChatMessage;
        private ICommand viewShare;
        private ICommand settings;
        private ICommand editShares;
        private ICommand changeAvatar;
        private ICommand viewQueue;
        private ICommand closing;
        private object logView;
        private object selectedClient;
        private byte[] avatar;
        private string networkInfo;
        private string nickname;
        private string description;
        private string windowTitle;
        private Node node;
        private SafeObservable<Node> peers;

        public MainWindowViewModel(IMainWindow view)
            : base(view)
        {

        }

        public bool AllowClose { set; get; }

        public SafeObservable<Node> Peers
        {
            get { return peers; }
            set
            {
                peers = value;
                RaisePropertyChanged("Peers");
            }
        }

        public Node Node
        {
            get { return node; }
            set
            {
                node = value;
                RaisePropertyChanged("Node");
            }
        }

        public byte[] Avatar
        {
            get { return avatar; }
            set
            {
                avatar = value;
                RaisePropertyChanged("Avatar");
            }
        }

        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                windowTitle = value;
                RaisePropertyChanged("WindowTitle");
            }
        }

        public string NetworkStatus
        {
            get { return networkInfo; }
            set
            {
                networkInfo = value;
                RaisePropertyChanged("NetworkStatus");
            }
        }

        public object LogView
        {
            get { return logView; }
            set
            {
                logView = value;
                RaisePropertyChanged("LogView");
            }
        }

        public string CurrentChatMessage
        {
            get { return currentChatMessage; }
            set
            {
                currentChatMessage = value;
                RaisePropertyChanged("CurrentChatMessage");
            }
        }

        public string Nickname
        {
            get { return nickname; }
            set
            {
                nickname = value;
                RaisePropertyChanged("Nickname");
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                RaisePropertyChanged("Description");
            }
        }



        public SafeObservable<Session> Sessions
        {
            get { return sessions; }
            set
            {
                sessions = value;
                RaisePropertyChanged("Sessions");
            }
        }

        public SafeObservable<string> ChatMessages
        {
            get { return chatList; }
            set
            {
                chatList = value;
                RaisePropertyChanged("ChatMessages");
            }
        }


        public ICommand Closing
        {
            get { return closing; }
            set
            {
                closing = value;
                RaisePropertyChanged("Closing");
            }
        }

        public ICommand ViewQueue
        {
            get { return viewQueue; }
            set
            {
                viewQueue = value;
                RaisePropertyChanged("ViewQueue");
            }
        }

        public ICommand EditShares
        {
            get { return editShares; }
            set
            {
                editShares = value;
                RaisePropertyChanged("EditShares");
            }
        }

        public ICommand SendChatMessage
        {
            get { return sendChatMessage; }
            set
            {
                sendChatMessage = value;
                RaisePropertyChanged("SendChatMessage");
            }
        }

        public ICommand Settings
        {
            get { return settings; }
            set
            {
                settings = value;
                RaisePropertyChanged("Settings");
            }
        }

        public ICommand ViewShare
        {
            get { return viewShare; }
            set
            {
                viewShare = value;
                RaisePropertyChanged("ViewShare");
            }
        }

        public ICommand ChangeAvatar
        {
            get { return changeAvatar; }
            set
            {
                changeAvatar = value;
                RaisePropertyChanged("ChangeAvatar");
            }
        }

        

        public object SelectedClient
        {
            get { return selectedClient; }
            set
            {
                selectedClient = value;
                RaisePropertyChanged("SelectedClient");
            }
        }


        

        public void Show()
        {
            ViewCore.Show();
        }

        public void Close()
        {
            ViewCore.Close();
        }

        public void AddWindow(string title, object content)
        {
            ViewCore.AddWindow(title, content);
        }

        public Dispatcher Dispatcher
        {
            get { return ViewCore.Dispatcher; }
        }
    }
}
