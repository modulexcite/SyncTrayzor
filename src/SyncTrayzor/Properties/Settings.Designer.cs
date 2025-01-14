﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SyncTrayzor.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://syncthing.antonymale.co.uk/version-check")]
        public string UpdateApiUrl {
            get {
                return ((string)(this["UpdateApiUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://github.com/canton7/SyncTrayzor")]
        public string HomepageUrl {
            get {
                return ((string)(this["HomepageUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2000")]
        public int DirectoryWatcherBackoffMilliseconds {
            get {
                return ((int)(this["DirectoryWatcherBackoffMilliseconds"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3000")]
        public int DirectoryWatcherFolderExistenceCheckMilliseconds {
            get {
                return ((int)(this["DirectoryWatcherFolderExistenceCheckMilliseconds"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://github.com/canton7/SyncTrayzor/issues")]
        public string IssuesUrl {
            get {
                return ((string)(this["IssuesUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnableAutostartOnFirstStart {
            get {
                return ((bool)(this["EnableAutostartOnFirstStart"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int CefRemoteDebuggingPort {
            get {
                return ((int)(this["CefRemoteDebuggingPort"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Portable")]
        public global::SyncTrayzor.Services.Config.SyncTrayzorVariant Variant {
            get {
                return ((global::SyncTrayzor.Services.Config.SyncTrayzorVariant)(this["Variant"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("43200")]
        public int UpdateCheckIntervalSeconds {
            get {
                return ((int)(this["UpdateCheckIntervalSeconds"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("600")]
        public int SyncthingConnectTimeoutSeconds {
            get {
                return ((int)(this["SyncthingConnectTimeoutSeconds"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<PathConfiguration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <LogFilePath>%EXEPATH%\logs</LogFilePath>
  <ConfigurationFilePath>%EXEPATH%\data\config.xml</ConfigurationFilePath>
  <ConfigurationFileBackupPath>%EXEPATH%\data\config-backups</ConfigurationFileBackupPath>
  <CefCachePath>%EXEPATH%\data\cef\cache</CefCachePath>
</PathConfiguration>")]
        public global::SyncTrayzor.Services.Config.PathConfiguration PathConfiguration {
            get {
                return ((global::SyncTrayzor.Services.Config.PathConfiguration)(this["PathConfiguration"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool EnforceSingleProcessPerUser {
            get {
                return ((bool)(this["EnforceSingleProcessPerUser"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<Configuration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""6"">
  <ShowTrayIconOnlyOnClose>false</ShowTrayIconOnlyOnClose>
  <MinimizeToTray>false</MinimizeToTray>
  <CloseToTray>true</CloseToTray>
  <ShowDeviceConnectivityBalloons>true</ShowDeviceConnectivityBalloons>
  <ShowSynchronizedBalloonEvenIfNothingDownloaded>false</ShowSynchronizedBalloonEvenIfNothingDownloaded>
  <SyncthingAddress>localhost:8384</SyncthingAddress>
  <StartSyncthingAutomatically>true</StartSyncthingAutomatically>
  <SyncthingCommandLineFlags />
  <SyncthingEnvironmentalVariables />
  <SyncthingUseCustomHome>true</SyncthingUseCustomHome>
  <SyncthingDenyUpgrade>false</SyncthingDenyUpgrade>
  <SyncthingPriorityLevel>Normal</SyncthingPriorityLevel>
  <Folders />
  <NotifyOfNewVersions>true</NotifyOfNewVersions>
  <ObfuscateDeviceIDs>true</ObfuscateDeviceIDs>
  <UseComputerCulture>true</UseComputerCulture>
  <SyncthingConsoleHeight>100</SyncthingConsoleHeight>
  <SyncthingWebBrowserZoomLevel>0</SyncthingWebBrowserZoomLevel>
  <LastSeenInstallCount>0</LastSeenInstallCount>
  <SyncthingPath>%EXEPATH%\data\syncthing.exe</SyncthingPath>
  <SyncthingCustomHomePath>%EXEPATH%\data\syncthing</SyncthingCustomHomePath>
  <DisableHardwareRendering>false</DisableHardwareRendering>
</Configuration>")]
        public global::SyncTrayzor.Services.Config.Configuration DefaultUserConfiguration {
            get {
                return ((global::SyncTrayzor.Services.Config.Configuration)(this["DefaultUserConfiguration"]));
            }
        }
    }
}
