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
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using log4net;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin
{
    [RunInstaller(true)]
    public partial class OctaneTfsPluginInstaller : System.Configuration.Install.Installer
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public OctaneTfsPluginInstaller()
        {
            InitializeComponent();
            LogUtils.ConfigureLog4NetForPluginMode(false);
        }


        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            DoNamespaceReservation();

            StartTFSJobAgentService();

            StartConfigurator();
        }

        private static void StartTFSJobAgentService()
        {
            LogUtils.WriteWindowsEvent("Starting service...", EventLogEntryType.Information, "ALM Octane Setup");
            var sc = new ServiceController("TFSJobAgent");
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running);
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
            String msg = "";
            try
            {
                string output = ExecuteCommand("netsh", "http show urlacl");
                string url = "http://+:4567/";
                if (!output.Contains(url))
                {
                    String addCommand = $"http add urlacl url={url} user=Everyone";
                    output = ExecuteCommand("netsh", addCommand);
                }
                else
                {
                    msg = $"Namespace Reservation already exist to {url}";
                    LogUtils.WriteWindowsEvent(msg, EventLogEntryType.Warning, "ALM Octane Setup");
                    Log.Warn(msg);
                    return;
                }

                if (!output.Contains("reservation successfully added"))
                {
                    msg = $"Error adding Namespace reservation, restserver will not work!. Error = {output}";
                    LogUtils.WriteWindowsEvent(msg, EventLogEntryType.Error, "ALM Octane Setup");
                    Log.Error(msg);
                }

                msg = $"Namespace reservation for rest server succesfully added  to {url}";
                LogUtils.WriteWindowsEvent(msg, EventLogEntryType.Information, "ALM Octane Setup");
                Log.Warn(msg);
            }
            catch (Exception e)
            {
                msg = $"Failed to do DoNamespaceReservation :  {e.Message}";
                LogUtils.WriteWindowsEvent(msg, EventLogEntryType.Information, "ALM Octane Setup");
                Log.Error(msg);
            }

            //see all reservations
            //netsh http show urlacl
            //delete reservation
            //netsh http delete urlacl url = http://+:4567/

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
            base.OnBeforeUninstall(savedState);

            LogUtils.WriteWindowsEvent("Stopping service...", EventLogEntryType.Information, "ALM Octane Setup");
            var sc = new ServiceController("TFSJobAgent");
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
            }

        }

        protected override void OnAfterUninstall(IDictionary savedState)
        {
            base.OnAfterUninstall(savedState);

            LogUtils.WriteWindowsEvent("Starting service...", EventLogEntryType.Information, "ALM Octane Setup");
            var sc = new ServiceController("TFSJobAgent");
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running);

        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            base.OnBeforeInstall(savedState);
            LogUtils.WriteWindowsEvent("Stopping service...", EventLogEntryType.Information, "ALM Octane Setup");
            var sc = new ServiceController("TFSJobAgent");
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }




    }
}
