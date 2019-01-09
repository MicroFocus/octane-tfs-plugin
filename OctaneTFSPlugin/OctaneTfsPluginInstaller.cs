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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin
{
    [RunInstaller(true)]
    public partial class OctaneTfsPluginInstaller : System.Configuration.Install.Installer
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string SERVICE_NAME = "TFSJobAgent";

        public OctaneTfsPluginInstaller()
        {
            InitializeComponent();
            LogUtils.ConfigureLog4NetForSetup();
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            Log.Warn("OnAfterInstall");
            Log.Warn("targetdir=" + GetInstallationPath());

            base.OnAfterInstall(savedState);

            DoUrlReservation();

            StartTfsJobAgentService();

            StartConfigurator();
        }

        private static void DoUrlReservation()
        {
            Log.Warn("Starting UrlReservation");
            Task taskA = new Task(() => UrlReservation.DoUrlReservation());
            taskA.Start();
        }

        private static void StartTfsJobAgentService()
        {
            Log.Warn("Starting TFSJobAgent service...");
            var sc = new ServiceController(SERVICE_NAME);
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running);
        }

        private static void StopTfsJobAgentService()
        {
            Log.Warn("Stopping TFSJobAgent service...");
            var sc = new ServiceController(SERVICE_NAME);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }

        private void StartConfigurator()
        {
            Log.Warn("StartConfigurator");

           
            string path = Path.Combine(GetInstallationPath(), "MicroFocus.Adm.Octane.CiPlugins.Tfs.ConfigurationLauncher.exe");

            Process.Start(path);
        }

        /// <summary>
        /// Available in install step only
        /// </summary>
        /// <returns></returns>
        private string GetInstallationPath()
        {
            return this.Context.Parameters["targetdir"];
        }

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            Log.Warn("OnBeforeUninstall");

            base.OnBeforeUninstall(savedState);

            StopTfsJobAgentService();
        }

        protected override void OnAfterUninstall(IDictionary savedState)
        {
            Log.Warn("OnAfterUninstall");

            base.OnAfterUninstall(savedState);

            StartTfsJobAgentService();
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            Log.Warn("OnBeforeInstall");

            base.OnBeforeInstall(savedState);

            StopTfsJobAgentService();
        }
    }
}
