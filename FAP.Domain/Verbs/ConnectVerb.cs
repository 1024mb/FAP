﻿#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.

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

using FAP.Network.Entities;
using Newtonsoft.Json;

namespace FAP.Domain.Verbs
{
    public class ConnectVerb : BaseVerb, IVerb
    {
        public string Address { set; get; }
        public string Secret { set; get; }
        public ClientType ClientType { set; get; }

        [JsonIgnore]
        public string OverlordID { set; get; }

        #region IVerb Members

        public NetworkRequest CreateRequest()
        {
            var r = new NetworkRequest();
            r.Verb = "CONNECT";
            r.Data = Serialize(this);
            return r;
        }

        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            var verb = Deserialise<ConnectVerb>(r.Data);
            Address = verb.Address;
            ClientType = verb.ClientType;
            Secret = verb.Secret;
            ClientType = verb.ClientType;
            OverlordID = r.OverlordID;
            r.Data = string.Empty;
            return r;
        }

        public bool ReceiveResponse(NetworkRequest r)
        {
            return true;
        }

        #endregion
    }
}