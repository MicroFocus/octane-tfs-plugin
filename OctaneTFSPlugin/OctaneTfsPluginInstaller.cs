using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.ServiceProcess;
using System.Text;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using MicroFocus.Ci.Tfs.Octane.Tools;

namespace MicroFocus.Ci.Tfs.Core
{
    [RunInstaller(true)]
    public partial class OctaneTfsPluginInstaller : System.Configuration.Install.Installer
    {
        public OctaneTfsPluginInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);


            var octaneServerUrl = Context.Parameters["OctaneServerUrl"];
            var clientId = Context.Parameters["ClientId"];
            var clientSecret = Context.Parameters["ClientSecret"];
            var instanceId = Context.Parameters["InstanceId"];
            var pat = Context.Parameters["PAT"];
            var tfsLocation = Context.Parameters["TfsLocation"];
            var conDetails =
                new ConnectionDetails(octaneServerUrl, clientId, clientSecret, tfsLocation, Guid.Parse(instanceId))
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
