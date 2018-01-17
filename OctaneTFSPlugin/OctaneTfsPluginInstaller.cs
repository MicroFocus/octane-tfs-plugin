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
            var path= Path.Combine(instpath, "OctaneTFSPluginConfiguratorUI.exe");
            System.Diagnostics.Process.Start(path);
        }
    }
}
