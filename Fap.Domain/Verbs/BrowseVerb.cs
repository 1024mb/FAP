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
using Fap.Network.Entity;
using Fap.Domain.Entity;
using System.IO;
using Fap.Network;

namespace Fap.Domain.Verbs
{
    public class BrowseVerb : VerbBase, IVerb
    {
        private List<FileSystemEntity> results = new List<FileSystemEntity>();

        private Model model;

        public BrowseVerb(Model m)
        {
            model = m;
        }


        public Network.Entity.Request CreateRequest()
        {
            Request request = new Request();
            request.Command = "BROWSE";
            request.Param = Path;
            request.AdditionalHeaders.Add("ID", model.Node.ID);
            return request;
        }

        public Network.Entity.Response ProcessRequest(Network.Entity.Request r)
        {
            Response response = new Response();
            response.RequestID = r.RequestID;
            if (string.IsNullOrEmpty(r.Param))
            {
                //If we are given no path then provide a list of virtual directories
                lock (model.Shares)
                {
                    foreach (var share in model.Shares.ToList().OrderBy(sh => sh.Name))
                    {
                        response.AdditionalHeaders.Add(share.Name, "D|0|0");
                    }
                }
            }
            else
            {
                try
                {
                    string path = string.Empty;
                    if (ReplacePath(r.Param, out path))
                    {
                        DirectoryInfo directory = new DirectoryInfo(path);
                        DirectoryInfo[] directories = directory.GetDirectories();
                        foreach (var dir in directories)
                            response.AdditionalHeaders.Add(dir.Name, "D|0|0" + dir.LastWriteTime.ToFileTime());

                        FileInfo[] files = directory.GetFiles();
                        foreach (var file in files)
                            response.AdditionalHeaders.Add(file.Name, "F|" + file.Length + "|" + file.LastWriteTime.ToFileTime());
                    }
                }
                catch { }
            }
            return response;
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            Results.Clear();
            foreach (var header in r.AdditionalHeaders)
            {
                FileSystemEntity fse = new FileSystemEntity();
                fse.Path = Path;
                fse.Name = header.Key;
                string[] split = header.Value.Split('|');
                if (split.Length == 3)
                {
                    fse.IsFolder = split[0] == "D";
                    long size = 0;
                    long.TryParse(split[1], out size);
                    fse.Size = size;
                    long.TryParse(split[2], out size);
                    if (size != 0)
                        fse.LastModified = DateTime.FromFileTime(size);
                    results.Add(fse);
                }
            }

            return true;
        }

        private bool ReplacePath(string input, out string output)
        {
            string[] split = input.Split('\\');
            if (split.Length > 0)
            {
                var share = model.Shares.Where(s => s.Name == split[0]).FirstOrDefault();
                if (null != share)
                {
                    output = share.Path + "\\" + input.Substring(share.Name.Length, input.Length - share.Name.Length);
                    return true;
                }
            }
            output = string.Empty;
            return false;
        }

        public string Path { set; get; }

        public List<FileSystemEntity> Results
        {
            get { return results; }
        }
    }
}