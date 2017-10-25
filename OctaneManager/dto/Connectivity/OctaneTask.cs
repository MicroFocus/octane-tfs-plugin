using System;
using System.Net.Http;
using Newtonsoft.Json;

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
