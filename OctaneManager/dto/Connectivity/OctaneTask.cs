using System;
using System.Net.Http;
using Newtonsoft.Json;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Connectivity
{
    internal class OctaneTask : OctaneTaskBase
    {        

        [JsonProperty("method")]        
        public HttpMethodEnum Method { get; set; }

     
        [JsonProperty("url")]
        public Uri ResultUrl { get; set; }
    }
}
