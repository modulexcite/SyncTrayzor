﻿using NLog;
using SyncTrayzor.Services.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SyncTrayzor.SyncThing
{
    public enum SyncThingExitStatus
    {
        // From https://github.com/syncthing/syncthing/blob/master/cmd/syncthing/main.go#L67
        Success = 0,
        Error = 1,
        NoUpgradeAvailable = 2,
        Restarting = 3,
        Upgrading = 4
    }

    public class ProcessStoppedEventArgs : EventArgs
    {
        public SyncThingExitStatus ExitStatus { get; }

        public ProcessStoppedEventArgs(SyncThingExitStatus exitStatus)
        {
            this.ExitStatus = exitStatus;
        }
    }

    public interface ISyncThingProcessRunner : IDisposable
    {
        string ExecutablePath { get; set; }
        string ApiKey { get; set; }
        string HostAddress { get; set; }
        string CustomHomeDir { get; set; }
        List<string> CommandLineFlags { get; set; }
        IDictionary<string, string> EnvironmentalVariables { get; set; }
        bool DenyUpgrade { get; set; }
        SyncThingPriorityLevel SyncthingPriorityLevel { get; set; }
        bool HideDeviceIds { get; set; }

        event EventHandler Starting;
        event EventHandler ProcessRestarted;
        event EventHandler<MessageLoggedEventArgs> MessageLogged;
        event EventHandler<ProcessStoppedEventArgs> ProcessStopped;

        void Start();
        void Kill();
        void KillAllSyncthingProcesses();
    }

    public class SyncThingProcessRunner : ISyncThingProcessRunner
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] defaultArguments = new[] { "-no-browser", "-no-restart" };
        // Leave just the first set of digits, removing everything after it
        private static readonly Regex deviceIdHideRegex = new Regex(@"-[0-9A-Z]{7}-[0-9A-Z]{7}-[0-9A-Z]{7}-[0-9A-Z]{7}-[0-9A-Z]{7}-[0-9A-Z]{7}-[0-9A-Z]{7}");

        private static readonly Dictionary<SyncThingPriorityLevel, ProcessPriorityClass> priorityMapping = new Dictionary<SyncThingPriorityLevel, ProcessPriorityClass>()
        {
            { SyncThingPriorityLevel.AboveNormal, ProcessPriorityClass.AboveNormal },
            { SyncThingPriorityLevel.Normal, ProcessPriorityClass.Normal },
            { SyncThingPriorityLevel.BelowNormal, ProcessPriorityClass.BelowNormal },
            { SyncThingPriorityLevel.Idle, ProcessPriorityClass.Idle },
        };

        private readonly object processLock = new object();
        private Process process;

        public string ExecutablePath { get; set; }
        public string ApiKey { get; set; }
        public string HostAddress { get; set; }
        public string CustomHomeDir { get; set; }
        public List<string> CommandLineFlags { get; set; } = new List<string>();
        public IDictionary<string, string> EnvironmentalVariables { get; set; } = new Dictionary<string, string>();
        public bool DenyUpgrade { get; set; }
        public SyncThingPriorityLevel SyncthingPriorityLevel { get; set; }
        public bool HideDeviceIds { get; set; }

        public event EventHandler Starting;
        public event EventHandler ProcessRestarted;
        public event EventHandler<MessageLoggedEventArgs> MessageLogged;
        public event EventHandler<ProcessStoppedEventArgs> ProcessStopped;

        public SyncThingProcessRunner()
        {
        }

        public void Start()
        {
            logger.Debug("SyncThingProcessRunner.Start called");
            // This might cause our config to be set...
            this.OnStarting();

            logger.Info("Starting syncthing: {0}", this.ExecutablePath);

            if (!File.Exists(this.ExecutablePath))
                throw new Exception($"Unable to find Syncthing at path {this.ExecutablePath}");

            var processStartInfo = new ProcessStartInfo()
            {
                FileName = this.ExecutablePath,
                Arguments = String.Join(" ", this.GenerateArguments()),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            if (this.DenyUpgrade)
                processStartInfo.EnvironmentVariables["STNOUPGRADE"] = "1";
            foreach (var kvp in this.EnvironmentalVariables)
            {
                processStartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
            }

            lock (this.processLock)
            {
                this.KillInternal();

                this.process = Process.Start(processStartInfo);

                try
                {
                    this.process.PriorityClass = priorityMapping[this.SyncthingPriorityLevel];
                }
                catch (InvalidOperationException e)
                {
                    // This can happen if syncthing.exe stops really really quickly (see #150)
                    // We shouldn't crash out: instead, keep going and see what the exit code was
                    logger.Warn("Failed to set process priority", e);
                }

                this.process.EnableRaisingEvents = true;
                this.process.OutputDataReceived += (o, e) => this.DataReceived(e.Data);
                this.process.ErrorDataReceived += (o, e) => this.DataReceived(e.Data);

                this.process.BeginOutputReadLine();
                this.process.BeginErrorReadLine();

                this.process.Exited += (o, e) => this.OnProcessExited();
            }
        }

        public void Kill()
        {
            logger.Info("Killing Syncthing process");
            lock (this.processLock)
            {
                this.KillInternal();
            }
        }

        // MUST BE CALLED FROM WITHIN A LOCK!
        private void KillInternal()
        {
            if (this.process != null)
            {
                try
                {
                    this.process.Kill();
                    this.process = null;
                }
                // These can happen in rare cases, and we don't care. See the docs for Process.Kill
                catch (Win32Exception e) { logger.Warn("KillInternal failed with an error", e); }
                catch (InvalidOperationException e) { logger.Warn("KillInternal failed with an error", e); }
            }
        }

        private IEnumerable<string> GenerateArguments()
        {
            var args = new List<string>(defaultArguments)
            {
                $"-gui-apikey=\"{this.ApiKey}\"",
                $"-gui-address=\"{this.HostAddress}\""
            };

            if (!String.IsNullOrWhiteSpace(this.CustomHomeDir))
                args.Add($"-home=\"{this.CustomHomeDir}\"");

            args.AddRange(this.CommandLineFlags);

            return args;
        }

        private void DataReceived(string data)
        {
            if (!String.IsNullOrWhiteSpace(data))
            {
                if (this.HideDeviceIds)
                    data = deviceIdHideRegex.Replace(data, "");
                this.OnMessageLogged(data);
            }
        }

        public void Dispose()
        {
            lock (this.processLock)
            {
                this.KillInternal();
            }
        }

        private void OnProcessExited()
        {
            SyncThingExitStatus exitStatus;
            lock (this.processLock)
            {
                exitStatus = this.process == null ? SyncThingExitStatus.Success : (SyncThingExitStatus)this.process.ExitCode;
                this.process = null;
            }

            logger.Info("Syncthing process stopped with exit status {0}", exitStatus);
            if (exitStatus == SyncThingExitStatus.Restarting || exitStatus == SyncThingExitStatus.Upgrading)
            {
                logger.Info("Syncthing process requested restart, so restarting");
                this.OnProcessRestarted();
                this.Start();
            }
            else
            {
                this.OnProcessStopped(exitStatus);
            }
        }

        private void OnStarting()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);
        }

        private void OnProcessStopped(SyncThingExitStatus exitStatus)
        {
            this.ProcessStopped?.Invoke(this, new ProcessStoppedEventArgs(exitStatus));
        }

        private void OnProcessRestarted()
        {
            this.ProcessRestarted?.Invoke(this, EventArgs.Empty);
        }

        private void OnMessageLogged(string logMessage)
        {
            logger.Debug(logMessage);
            this.MessageLogged?.Invoke(this, new MessageLoggedEventArgs(logMessage));
        }

        public void KillAllSyncthingProcesses()
        {
            logger.Debug("Kill all Syncthing processes");
            foreach (var process in Process.GetProcessesByName("syncthing"))
            {
                process.Kill();
            }
        }
    }
}
