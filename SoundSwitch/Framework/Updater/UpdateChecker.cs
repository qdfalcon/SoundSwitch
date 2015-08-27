﻿/********************************************************************
* Copyright (C) 2015 Antoine Aflalo
* 
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SoundSwitch.Framework.Updater
{
    internal class UpdateChecker
    {
        private static readonly string UserAgent =
            $"Mozilla/5.0 (compatible; Windows NT {Environment.OSVersion.Version}; SoundSwitch/{Application.ProductVersion}; +https://github.com/Belphemur/SoundSwitch)";

        private readonly Uri _releaseUrl;
        private readonly WebClient _webClient = new WebClient();
        public EventHandler<NewReleaseEvent> UpdateAvailable;

        private UpdateChecker(Uri releaseUrl)
        {
            _releaseUrl = releaseUrl;
            _webClient.DownloadStringCompleted += DownloadStringCompleted;
        }

        private void DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                AppLogger.Log.Error("Exception while getting release ", e.Error);
                return;
            }

            var serverRelease = JsonConvert.DeserializeObject<GitHubRelease>(e.Result);
            var version = new Version(serverRelease.tag_name.Substring(1));
            var changelog = Regex.Split(serverRelease.body, "\r\n|\r|\n");
            try
            {
                var installer = serverRelease.assets.First(asset => asset.name.EndsWith(".exe"));
                var release = new Release(version,installer, serverRelease.name);
                release.Changelog.AddRange(changelog);
                UpdateAvailable?.Invoke(this, new NewReleaseEvent(release));
            }
            catch (Exception ex)
            {
                AppLogger.Log.Error("Exception while getting release ", ex);
            }
        }
        /// <summary>
        /// Check for update 
        /// </summary>
        public void CheckForUpdate()
        {
            _webClient.Headers.Add("User-Agent", UserAgent);
            Task.Factory.StartNew(() => _webClient.DownloadStringAsync(_releaseUrl));
        }

        public class NewReleaseEvent : EventArgs
        {
            public Release Release { get; private set; }

            public NewReleaseEvent(Release release)
            {
                Release = release;
            }
        }
    }
}