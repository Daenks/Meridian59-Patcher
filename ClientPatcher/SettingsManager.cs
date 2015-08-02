﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Newtonsoft.Json;
using System.IO;
using System.Security.AccessControl;
using System.Net;

namespace ClientPatcher
{
    class SettingsManager
    {
        //Where to download latest setting from
        public const string SettingsUrl = "http://ww1.openmeridian.org/settings.php";

        private readonly string _settingsPath; //Path to JSON file settings.txt
        private readonly string _settingsFile;

        public List<PatcherSettings> Servers { get; set; } //Loaded from settings.txt, or generated on first run and then saved.

        public SettingsManager()
        {
            _settingsPath = "C:\\Program Files\\Open Meridian";
            _settingsFile = "\\settings.txt";
        }

        public void Refresh()
        {
            LoadSettings();
            MergeWebSettings(GetNewSettings());
            SaveSettings();
        }
        /// <summary>
        /// Downloads the list of available servers from the open meridian web server using JSON encoded PatcherSettings objects.
        /// </summary>
        public List<PatcherSettings> GetNewSettings()
        {
            try
            {
                var myClient = new WebClient();
                
                //Download the settings from the web, store them in a list.
                var webSettingsList =
                    JsonConvert.DeserializeObject<List<PatcherSettings>>(myClient.DownloadString(SettingsUrl)); 
                return webSettingsList;
                
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unable to download settings from server." + e);
            }         
        }
        /// <summary>
        /// Merges a list of PatcherSettings objects with the currently loaded settings
        /// </summary>
        /// <param name="webSettingsList">List of PatcherSetting objects to merge</param>
        public void MergeWebSettings(List<PatcherSettings> webSettingsList )
        {
            foreach (PatcherSettings webProfile in webSettingsList) //Loop through loaded settings from settings.txt
            {
                //find the matching local profile by Guid
                PatcherSettings localProfile = Servers.FirstOrDefault(i => i.Guid == webProfile.Guid);
                //if a local match, update, else, add a new local profile
                if (localProfile != null)
                {
                    //dont update all fields on purpose. user retains control of paths, which client to use.
                    localProfile.PatchBaseUrl = webProfile.PatchBaseUrl;
                    localProfile.PatchInfoUrl = webProfile.PatchInfoUrl;
                    localProfile.ServerName = webProfile.ServerName;
                    localProfile.FullInstallUrl = webProfile.FullInstallUrl;
                    localProfile.AccountCreationUrl = webProfile.AccountCreationUrl;
                }
                else
                {
                    Servers.Add(webProfile);
                }
            }
        }
        /// <summary>
        /// Load the settings.txt file and use json deserialization to create an array of PatcherSettings objects.
        /// Otherwise, download the settings from the web.
        /// </summary>
        public void LoadSettings()
        {
            if (File.Exists(_settingsPath + _settingsFile))
            {
                StreamReader file = File.OpenText(_settingsPath+_settingsFile); //Open the file

                Servers = JsonConvert.DeserializeObject<List<PatcherSettings>>(file.ReadToEnd()); //convert
                file.Close(); //close
            }
            else
            {
                Servers = GetNewSettings();
                GrantAccess();
            }
        }
        /// <summary>
        /// JSON Serialize our PatcherSettings and write them to settings.txt
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                using (var sw = new StreamWriter(_settingsPath + _settingsFile)) //open file
                {
                    sw.Write(JsonConvert.SerializeObject(Servers, Formatting.Indented)); //write shit
                }
            }
            catch (Exception e)
            {
                
                throw new Exception("Unable to SaveSettings()" + e);
            }
            
        }

        //used when adding from form
        public void AddProfile(string clientfolder, string patchbaseurl, string patchinfourl, string servername, bool isdefault = false, bool usedotnetclient = false)
        {
            var ps = new PatcherSettings
            {
                ClientFolder = clientfolder,
                PatchBaseUrl = patchbaseurl,
                PatchInfoUrl = patchinfourl,
                ServerName = servername,
                Default = isdefault,
                UseDotNetClient = usedotnetclient
            };
            if (isdefault)
            {
                foreach (PatcherSettings patcherSettingse in Servers.FindAll(s => s.Default))
                {
                    patcherSettingse.Default = false;
                }
            }
            Servers.Add(ps);
            SaveSettings();
            LoadSettings();
        }
        public void AddProfile(PatcherSettings newprofile)
        {
            if (newprofile.Default)
            {
                foreach (PatcherSettings patcherSettingse in Servers.FindAll(s => s.Default))
                {
                    patcherSettingse.Default = false;
                }
            }
            Servers.Add(newprofile);
            SaveSettings();
            LoadSettings();
        }

        public PatcherSettings FindByName(string name)
        {
            return Servers.Find(x => x.ServerName == name);
        }

        public PatcherSettings GetDefault()
        {
            return Servers.Find(x => x.Default);
        }
        /// <summary>
        /// Sets proper NTFS permissions for the patcher to operate
        /// </summary>
        private void GrantAccess()
        {
            try
            {
                var dSecurity = new DirectorySecurity();
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.Modify | FileSystemRights.Synchronize,
                                                                          InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                                          PropagationFlags.None, AccessControlType.Allow));
                dSecurity.SetAccessRuleProtection(false, true);
                Directory.CreateDirectory(_settingsPath, dSecurity);
            }
            catch (Exception e)
            {
                
                throw new Exception("Unable to GrantAccess()" + e);
            }
            
        }

    }
}
