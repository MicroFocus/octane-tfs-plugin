/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin
{
    [RunInstaller(true)]
    public partial class OctaneTfsPluginInstaller : System.Configuration.Install.Installer
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public OctaneTfsPluginInstaller()
        {
            InitializeComponent();
            SetupLog4NetForSetup();
        }


        protected override void OnAfterInstall(IDictionary savedState)
        {
            LogMessage("OnAfterInstall", EventLogEntryType.Information);

            base.OnAfterInstall(savedState);

            //DoNamespaceReservation();

            StartTfsJobAgentService();

            StartConfigurator();
        }

        private static void StartTfsJobAgentService()
        {
            LogMessage("Starting TFSJobAgent service...", EventLogEntryType.Information);
            var sc = new ServiceController("TFSJobAgent");
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running);
        }

        private static void StopTfsJobAgentService()
        {
            LogMessage("Stopping TFSJobAgent service...", EventLogEntryType.Information);
            var sc = new ServiceController("TFSJobAgent");
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }

        private void StartConfigurator()
        {
            var instpath = this.Context.Parameters["targetdir"];

            //start configurator
            var path = Path.Combine(instpath, "ALMOctaneTFSPluginConfiguratorUI.exe");
            Process.Start(path);
        }

        private static void DoNamespaceReservation()
        {
            LogMessage("Starting NamespaceReservation ...", EventLogEntryType.Information);
            //see all reservations : netsh http show urlacl
            //delete reservation   : netsh http delete urlacl url = http://+:4567/
            //add reservation      : netsh http add urlacl url = http://+:4567/ user=Everyone

            String msg = "";
            String command = "netsh";
            try
            {
                string output = ExecuteCommand(command, "http show urlacl");
                string url = "http://+:4567/";
                if (!output.Contains(url))
                {
                    String addCommand = $"http add urlacl url={url} user=Everyone";
                    output = ExecuteCommand(command, addCommand);
                    if (!output.Contains("reservation successfully added"))
                    {
                        msg = $"Error adding Namespace reservation, restserver will not work!. Error = {output}";
                        LogMessage(msg, EventLogEntryType.Error);
                    }
                    else
                    {
                        msg = $"Namespace reservation for rest server succesfully added  to {url}";
                        LogMessage(msg, EventLogEntryType.Information);
                    }
                }
                else
                {
                    msg = $"Namespace Reservation already exist to {url}";
                    LogMessage(msg, EventLogEntryType.Warning);
                    return;
                }
            }
            catch (Exception e)
            {
                msg = $"Failed to do DoNamespaceReservation :  {e.Message}";
                LogMessage(msg, EventLogEntryType.Information);
            }

        }

        private static void LogMessage(String msg, EventLogEntryType eventLogEntryType)
        {
            LogUtils.WriteWindowsEvent(msg, eventLogEntryType, "ALM Octane Setup");

            switch (eventLogEntryType)
            {
                case EventLogEntryType.Error:
                    Log.Error(msg);
                    break;
                case EventLogEntryType.Warning:
                    Log.Warn(msg);
                    break;
                default:
                    Log.Warn(msg);
                    break;
            }
        }

        private static string ExecuteCommand(string command, string argument)
        {
            var process = new Process();
            var startInfo =
                new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = command,
                    //Verb = "runas",
                    Arguments = argument,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();

            return output;
        }


        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            LogMessage("OnBeforeUninstall", EventLogEntryType.Information);

            base.OnBeforeUninstall(savedState);

            StopTfsJobAgentService();

        }

        protected override void OnAfterUninstall(IDictionary savedState)
        {
            LogMessage("OnBeforeUninstall", EventLogEntryType.Information);

            base.OnAfterUninstall(savedState);

            StartTfsJobAgentService();
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            LogMessage("OnBeforeUninstall", EventLogEntryType.Information);

            base.OnBeforeInstall(savedState);

            StopTfsJobAgentService();
        }

        public static void SetupLog4NetForSetup()
        {
            String path = @"C:\Users\Public\Documents\OctaneTfsPlugin\logs\Setup.log";
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = true;
            roller.File = path;
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 2;
            roller.MaximumFileSize = "3MB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            MemoryAppender memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = Level.Debug;
            hierarchy.Configured = true;
        }
    }
}
