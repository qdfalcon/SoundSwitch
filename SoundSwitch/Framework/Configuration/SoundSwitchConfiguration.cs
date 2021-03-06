﻿/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Common.WinApi.Keyboard;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.TrayIcon.Icon;
using SoundSwitch.Framework.TrayIcon.TooltipInfoManager.TootipInfo;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Localization;
using SoundSwitch.Localization.Factory;

namespace SoundSwitch.Framework.Configuration
{
    public class SoundSwitchConfiguration : ISoundSwitchConfiguration
    {
        public SoundSwitchConfiguration()
        {
            // Basic Settings
            FirstRun = true;
            SwitchForegroundProgram = false;

            // Audio Settings
            ChangeCommunications = false;
            NotificationSettings = NotificationTypeEnum.BannerNotification;
            TooltipInfo = TooltipInfoTypeEnum.Playback;
            CyclerType = DeviceCyclerTypeEnum.Available;

            // Update Settings
            UpdateCheckInterval = 3600 * 24; // 24 hours
            UpdateMode = UpdateMode.Notify;
            IncludeBetaVersions = false;

            // Language Settings
            Language = new LanguageFactory().GetWindowsLanguage();
            SelectedPlaybackDeviceListId = new HashSet<string>();
            SelectedRecordingDeviceListId = new HashSet<string>();
            PlaybackHotKey = new HotKey(Keys.F11, HotKey.ModifierKeys.Alt | HotKey.ModifierKeys.Control);
            RecordingHotKey = new HotKey(Keys.F7, HotKey.ModifierKeys.Alt | HotKey.ModifierKeys.Control);

            SelectedDevices = new HashSet<DeviceInfo>();
            SwitchIcon = IconChangerFactory.ActionEnum.Never;
            MigratedFields = new HashSet<string>();
        }


        public HashSet<string> SelectedPlaybackDeviceListId { get; }
        public HashSet<string> SelectedRecordingDeviceListId { get; }
        public HashSet<DeviceInfo> SelectedDevices { get; }
        public bool FirstRun { get; set; }
        public HotKey PlaybackHotKey { get; set; }
        public HotKey RecordingHotKey { get; set; }
        public bool ChangeCommunications { get; set; }
        public uint UpdateCheckInterval { get; set; }
        public UpdateMode UpdateMode { get; set; }
        public TooltipInfoTypeEnum TooltipInfo { get; set; }
        public DeviceCyclerTypeEnum CyclerType { get; set; }
        public NotificationTypeEnum NotificationSettings { get; set; }
        public Language Language { get; set; }
        public bool IncludeBetaVersions { get; set; }
        public string CustomNotificationFilePath { get; set; }
        [Obsolete]
        public bool KeepSystrayIcon { get; set; }
        public bool SwitchForegroundProgram { get; set; }
        public IconChangerFactory.ActionEnum SwitchIcon { get; set; }
        public HashSet<ProfileSetting> ProfileSettings { get; set; } = new HashSet<ProfileSetting>();
        
        /// <summary>
        /// Fields of the config that got migrated
        /// </summary>
        public HashSet<string> MigratedFields { get; set; }
        // Needed by Interface
        public string FileLocation { get; set; }
        /// <summary>
        /// Migrate configuration to a new schema
        /// </summary>
        public void Migrate()
        {
            if (SelectedPlaybackDeviceListId.Count > 0)
            {
                SelectedDevices.UnionWith(
                    SelectedPlaybackDeviceListId.Select((s => new DeviceInfo("", s, DataFlow.Render))));
                SelectedPlaybackDeviceListId.Clear();
            }

            if (SelectedRecordingDeviceListId.Count > 0)
            {
                SelectedDevices.UnionWith(
                    SelectedRecordingDeviceListId.Select((s => new DeviceInfo("", s, DataFlow.Capture))));
                SelectedRecordingDeviceListId.Clear();
            }

            if (NotificationSettings == NotificationTypeEnum.ToastNotification)
            {
                NotificationSettings = NotificationTypeEnum.BannerNotification;
            }

            if (!MigratedFields.Contains("KeepSystrayIcon"))
            {
                SwitchIcon = KeepSystrayIcon ? IconChangerFactory.ActionEnum.Never : IconChangerFactory.ActionEnum.Always;
                MigratedFields.Add("KeepSystrayIcon");
            }
            
        }

        public void Save()
        {

            Log.Debug("Saving configuration ", this);
            ConfigurationManager.SaveConfiguration(this);

        }

        public override string ToString()
        {
            return $"{GetType().Name}({FileLocation})";
        }
    }
}