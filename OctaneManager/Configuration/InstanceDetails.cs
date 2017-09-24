using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Configuration
{
    internal class InstanceDetails
    {
        private const string API_VERSION = "1";
        private const string SDK_VERSION = "1";
        private const string PLUGIN_VERSION = "1";

        public string ApiVersion => API_VERSION;

        public string SdkVersion => SDK_VERSION;

        public string PluginVersion => PLUGIN_VERSION;

        public string Type => "tfs";
        public Guid InstanceId { get; set; }

        public string SelfLocation { get; protected set; }

        public InstanceDetails(Guid instanceId, string selfLocation)
        {
            InstanceId = instanceId;
            SelfLocation = selfLocation;
        }

        public InstanceDetails(string selfLocation)
        {
            InstanceId = Guid.NewGuid();
            SelfLocation = selfLocation;
        }
    }
}
