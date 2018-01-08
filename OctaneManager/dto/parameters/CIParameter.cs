using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.dto.parameters
{
    public class CiParameter
    {
        [JsonProperty("type")]
        public string ParameterType { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("defaultValue")]
        public string DefaultValue { get; set; }
    }
}
