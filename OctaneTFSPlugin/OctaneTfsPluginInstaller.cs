using System;
using System.Collections;
using System.ComponentModel;
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

        protected override void OnBeforeInstall(IDictionary savedState)
        {                        
            base.OnBeforeInstall(savedState);

        }

        
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
          
            var octaneServerUrl = Context.Parameters["OctaneServerUrl"];
            var clientId = Context.Parameters["ClientId"];
            var clientSecret = Context.Parameters["ClientSecret"];
            var instanceId = Guid.NewGuid().ToString(); //Context.Parameters["InstanceId"];
            var pat = Context.Parameters["PAT"];
            var tfsLocation = ConnectionCreator.GetTfsLocationFromHostName();
            var conDetails =
                new ConnectionDetails(octaneServerUrl, clientId, clientSecret, tfsLocation, instanceId)
                {
                    Pat = pat
                };
            ConfigurationManager.WriteConfig(conDetails);                        
        }

        public override void Uninstall(IDictionary savedState)
        {
            //base.Uninstall(savedState);

            //var service = new ServiceController("TFSJobAgent");

            //if ((service.Status.Equals(ServiceControllerStatus.Running)) ||

            //    (service.Status.Equals(ServiceControllerStatus.StartPending)))
            //{
            //    service.Stop();
            //}
        }
    }
}
