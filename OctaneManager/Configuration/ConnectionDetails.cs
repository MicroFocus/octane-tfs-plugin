using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hpe.Nga.Api.Core.Entities;

namespace MicroFocus.Ci.Tfs.Octane.Configuration
{
    internal class ConnectionDetails
    {
        public string Host
        {
            get
            {
                var index = WebAppUrl.IndexOf("/", WebAppUrl.LastIndexOf(":", StringComparison.Ordinal),
                    StringComparison.Ordinal);
                var res = WebAppUrl.Substring(0, index + 1);
                return res;
            }
        }

        public int SharedSpace
        {
            get
            {
                var index = WebAppUrl.IndexOf("=", StringComparison.Ordinal);
                var res = int.Parse(WebAppUrl.Substring(index + 1));
                return res;
            }
        }

        public string WebAppUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public ConnectionDetails()
        {
        }

        public ConnectionDetails(string webAppUrl, string clientId, string clientSecret,Guid? instanceId = null)
        {
            WebAppUrl = webAppUrl;
            ClientId = clientId;
            ClientSecret = clientSecret;
            InstanceId = instanceId ?? Guid.NewGuid();
        }

        public Guid InstanceId { get; set; } = Guid.NewGuid();
    }
}
