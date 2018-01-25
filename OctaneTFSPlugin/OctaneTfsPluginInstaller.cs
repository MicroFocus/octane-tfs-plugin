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
using System.IO;
using System.Windows.Forms;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;

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

            var instpath = this.Context.Parameters["targetdir"];
            
            var path= Path.Combine(instpath, "ALMOctaneTFSPluginConfiguratorUI.exe");
            System.Diagnostics.Process.Start(path);
        }
    }
}
