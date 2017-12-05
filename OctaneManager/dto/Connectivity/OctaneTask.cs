using System;
using System.Net.Http;
using Newtonsoft.Json;
using MicroFocus.Ci.Tfs.Octane.Tools;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Connectivity
{
    internal class OctaneTask : OctaneTaskBase
    {        

        [JsonProperty("method")]        
        public HttpMethodEnum Method { get; set; }

     
        [JsonProperty("url")]
        public Uri ResultUrl { get; set; }
    }
}
