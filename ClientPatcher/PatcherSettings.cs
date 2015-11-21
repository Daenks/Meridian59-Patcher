﻿using Newtonsoft.Json;
using PatchListGenerator;

namespace ClientPatcher
{
    /// <summary>
    /// A set of settings about a specific Meridian 59 Server
    /// </summary>
    public class PatcherSettings
    {
        public string ServerName { get; set; }     //What do we call this profile?
        public string PatchInfoUrl { get; set; }   //Where is the file containing md5 hashes to compare?
        public string ClientFolder { get; set; }   //Where is the local copy of the client?
        public string PatchBaseUrl { get; set; }   //Where to download individual files?
        public string Guid { get; set; }           //Unique ID for a server profile (so they can be renamed by end users)
        public string FullInstallUrl { get; set; } //Path to a .zip file of the full client to download for first run
        public string AccountCreationUrl { get; set; } //URL to load when "Create Account" button is clicked.
        public bool Default { get; set; }          //Is this profile the default-selected at start up?
        public ClientType ClientType { get; set; } //Which client does this profile use?

        public PatcherSettings()
        {
            ServerName = "103";
            PatchInfoUrl = "http://ww1.openmeridian.org/103/patchinfo.txt";
            FullInstallUrl = "http://ww1.openmeridian.org/103/Meridian59.Client.Classic.zip";
            ClientFolder = "C:\\Program Files\\Open Meridian\\Meridian 103";
            PatchBaseUrl = "http://ww1.openmeridian.org/103/clientpatch";
            AccountCreationUrl = "http://ww1.openmeridian.org/103/acctcreate.php";
            Guid = "5AD1FB01-A84A-47D1-85B8-5F85FB0C201E";
            Default = true;
            ClientType = ClientType.Classic;
        }

        public PatcherSettings(string servername, string patchinfourl, string clientfolder, string patchbaseurl, string fullinstallurl, bool defaultserver = false, ClientType clientType = ClientType.Classic)
        {
            ServerName = servername;
            PatchInfoUrl = patchinfourl;
            ClientFolder = clientfolder;
            PatchBaseUrl = patchbaseurl;
            FullInstallUrl = fullinstallurl;
            Default = defaultserver;
            ClientType = clientType;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
