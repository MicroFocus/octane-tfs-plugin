using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Configuration
{
    internal static class ConfigurationManager
    {
        public static string ConfigurationFile => "octane.conf.json";
        public static ConnectionDetails Read()
        {
            ConnectionDetails res = null;
            using (var reader = new StreamReader(ConfigurationFile))
            {
                res = JsonConvert.DeserializeObject<ConnectionDetails>(reader.ReadToEnd());
            }

            return res;
        }                
    }
}
