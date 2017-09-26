using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Connectivity
{
    internal class OctaneTask : OctaneTaskBase
    {        

        [JsonProperty("method")]        
        public string Method { get; set; }

     
        [JsonProperty("url")]
        public Uri ResultUrl { get; set; }
    }
}
