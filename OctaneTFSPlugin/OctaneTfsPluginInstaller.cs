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
using System.ServiceProcess;
using System.Windows.Forms;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin
{
    [RunInstaller(true)]
    public partial class OctaneTfsPluginInstaller : System.Configuration.Install.Installer
    {
        public OctaneTfsPluginInstaller()
        {
            InitializeComponent();
        }


        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            LogUtils.WriteWindowsEvent("Starting service...", EventLogEntryType.Information, "ALM Octane Setup");
            var sc = new ServiceController("TFSJobAgent");
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running);

            var instpath = this.Context.Parameters["targetdir"];
            
            var path= Path.Combine(instpath, "ALMOctaneTFSPluginConfiguratorUI.exe");
            System.Diagnostics.Process.Start(path);

            var process = new System.Diagnostics.Process();
            var startInfo =
                new System.Diagnostics.ProcessStartInfo
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                    FileName = "cmd.exe",
                    Arguments = "/C netsh http add urlacl url=http://+:4567// user=Everyone",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
            process.StartInfo = startInfo;
            process.Start();

            var output = process.StandardOutput.ReadToEnd();

            if (!output.Contains("reservation successfully added"))
            {
                LogUtils.WriteWindowsEvent("error adding reservation, restserver will not work!", EventLogEntryType.Error, "ALM Octane Setup");
            }

            LogUtils.WriteWindowsEvent("Reservation for rest server succesfully added ", EventLogEntryType.Information, "ALM Octane Setup");
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
            LogUtils.WriteWindowsEvent("Stopping service...", EventLogEntryType.Information,"ALM Octane Setup");
            var sc = new ServiceController("TFSJobAgent");
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }

        
        

    }
}
