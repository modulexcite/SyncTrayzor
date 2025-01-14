﻿using NLog;
using Stylet;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SyncTrayzor.Services.UpdateManagement
{
    public class VersionIgnoredEventArgs : EventArgs
    {
        public Version IgnoredVersion { get;  }

        public VersionIgnoredEventArgs(Version ignoredVersion)
        {
            this.IgnoredVersion = ignoredVersion;
        }
    }

    public interface IUpdateManager
    {
        event EventHandler<VersionIgnoredEventArgs> VersionIgnored;
        Version LatestIgnoredVersion { get; set; }
        string UpdateCheckApiUrl { get; set; }
        bool CheckForUpdates { get; set; }
        TimeSpan UpdateCheckInterval { get; set; }

        Task<VersionCheckResults> CheckForAcceptableUpdateAsync();
    }

    public class UpdateManager : IUpdateManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IApplicationState applicationState;
        private readonly IApplicationWindowState applicationWindowState;
        private readonly IUserActivityMonitor userActivityMonitor;
        private readonly IUpdateCheckerFactory updateCheckerFactory;
        private readonly IProcessStartProvider processStartProvider;
        private readonly IUpdatePromptProvider updatePromptProvider;
        private readonly Func<IUpdateVariantHandler> updateVariantHandlerFactory;
        private readonly DispatcherTimer promptTimer;

        private readonly SemaphoreSlim versionCheckLock = new SemaphoreSlim(1, 1);

        private DateTime lastCheckedTime;
        private CancellationTokenSource toastCts;

        public event EventHandler<VersionIgnoredEventArgs> VersionIgnored;
        public Version LatestIgnoredVersion { get; set; }
        public string UpdateCheckApiUrl { get; set; }
        public TimeSpan UpdateCheckInterval { get; set; }

        private bool _checkForUpdates;
        public bool CheckForUpdates
        {
            get { return this._checkForUpdates; }
            set
            {
                if (this._checkForUpdates == value)
                    return;
                this._checkForUpdates = value;
                this.UpdateCheckForUpdates(value);
            }
        }

        public UpdateManager(
            IApplicationState applicationState,
            IApplicationWindowState applicationWindowState,
            IUserActivityMonitor userActivityMonitor,
            IUpdateCheckerFactory updateCheckerFactory,
            IProcessStartProvider processStartProvider,
            IUpdatePromptProvider updatePromptProvider,
            Func<IUpdateVariantHandler> updateVariantHandlerFactory)
        {
            this.applicationState = applicationState;
            this.applicationWindowState = applicationWindowState;
            this.userActivityMonitor = userActivityMonitor;
            this.updateCheckerFactory = updateCheckerFactory;
            this.processStartProvider = processStartProvider;
            this.updatePromptProvider = updatePromptProvider;
            this.updateVariantHandlerFactory = updateVariantHandlerFactory;

            this.promptTimer = new DispatcherTimer();
            this.promptTimer.Tick += this.PromptTimerElapsed;

            // Strategy time:
            // We'll prompt the user a fixed period after the computer starts up / resumes from sleep
            // We'll also check on a fixed interval since this point
            // We'll also check when the application is restored from tray

            this.applicationState.Startup += this.ApplicationStartup;
            this.applicationState.ResumeFromSleep += this.ResumeFromSleep;
            this.applicationWindowState.RootWindowActivated += this.RootWindowActivated;
        }

        private async void UpdateCheckForUpdates(bool checkForUpdates)
        {
            if (checkForUpdates)
            {
                this.RestartTimer();
                // Give them a minute to catch their breath
                await Task.Delay(TimeSpan.FromSeconds(30));
                if (this.UpdateCheckDue())
                    await this.CheckForUpdatesAsync();
            }
            else
            {
                this.promptTimer.IsEnabled = false;
            }
        }

        private async void ApplicationStartup(object sender, EventArgs e)
        {
            await this.CheckForUpdatesAsync();
        }

        private async void ResumeFromSleep(object sender, EventArgs e)
        {
            if (this.UpdateCheckDue())
                await this.CheckForUpdatesAsync();
        }

        private async void RootWindowActivated(object sender, ActivationEventArgs e)
        {
            if (this.toastCts != null)
                this.toastCts.Cancel();

            // Always check on root window activated
            await this.CheckForUpdatesAsync();
        }

        private async void PromptTimerElapsed(object sender, EventArgs e)
        {
            if (this.UpdateCheckDue())
                await this.CheckForUpdatesAsync();
        }

        private void OnVersionIgnored(Version ignoredVersion)
        {
            this.VersionIgnored?.Invoke(this, new VersionIgnoredEventArgs(ignoredVersion));
        }

        private bool UpdateCheckDue()
        {
            return DateTime.UtcNow - this.lastCheckedTime > this.UpdateCheckInterval;
        }

        private void RestartTimer()
        {
            this.promptTimer.IsEnabled = false;
            this.promptTimer.Interval = this.UpdateCheckInterval;
            this.promptTimer.IsEnabled = true;
        }

        private async Task CheckForUpdatesAsync()
        {
            if (!this.versionCheckLock.Wait(0))
                return;

            try
            {
                this.lastCheckedTime = DateTime.UtcNow;

                if (!this.CheckForUpdates)
                    return;

                this.RestartTimer();

                var variantHandler = this.updateVariantHandlerFactory();

                var updateChecker = this.updateCheckerFactory.CreateUpdateChecker(this.UpdateCheckApiUrl, variantHandler.VariantName);
                var checkResult = await updateChecker.CheckForAcceptableUpdateAsync(this.LatestIgnoredVersion);

                if (checkResult == null)
                    return;

                if (!await variantHandler.TryHandleUpdateAvailableAsync(checkResult))
                    return;

                VersionPromptResult promptResult;
                if (this.applicationState.HasMainWindow)
                {
                    promptResult = this.updatePromptProvider.ShowDialog(checkResult, variantHandler.CanAutoInstall);
                }
                else
                {
                    // If another application is fullscreen, don't bother
                    if (this.userActivityMonitor.IsWindowFullscreen())
                        return;

                    try
                    {
                        this.toastCts = new CancellationTokenSource();
                        promptResult = await this.updatePromptProvider.ShowToast(checkResult, variantHandler.CanAutoInstall, this.toastCts.Token);
                        this.toastCts = null;

                        // Special case
                        if (promptResult == VersionPromptResult.ShowMoreDetails)
                        {
                            this.applicationWindowState.EnsureInForeground();
                            promptResult = this.updatePromptProvider.ShowDialog(checkResult, variantHandler.CanAutoInstall);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        this.toastCts = null;
                        logger.Info("Update toast cancelled. Moving to a dialog");
                        promptResult = this.updatePromptProvider.ShowDialog(checkResult, variantHandler.CanAutoInstall);
                    }
                }

                switch (promptResult)
                {
                    case VersionPromptResult.InstallNow:
                        Debug.Assert(variantHandler.CanAutoInstall);
                        logger.Info("Auto-installing {0}", checkResult.NewVersion);
                        variantHandler.AutoInstall();
                        break;

                    case VersionPromptResult.Download:
                        logger.Info("Proceeding to download URL {0}", checkResult.DownloadUrl);
                        this.processStartProvider.StartDetached(checkResult.ReleasePageUrl);
                        break;

                    case VersionPromptResult.Ignore:
                        logger.Info("Ignoring version {0}", checkResult.NewVersion);
                        this.OnVersionIgnored(checkResult.NewVersion);
                        break;

                    case VersionPromptResult.RemindLater:
                        logger.Info("Not installing version {0}, but will remind later", checkResult.NewVersion);
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error("Error in UpdateManager.CheckForUpdatesAsync", e);
            }
            finally
            {
                this.versionCheckLock.Release();
            }
        }

        public Task<VersionCheckResults> CheckForAcceptableUpdateAsync()
        {
            var variantHandler = this.updateVariantHandlerFactory();
            var updateChecker = this.updateCheckerFactory.CreateUpdateChecker(this.UpdateCheckApiUrl, variantHandler.VariantName);
            return updateChecker.CheckForAcceptableUpdateAsync(this.LatestIgnoredVersion);
        }
    }
}
