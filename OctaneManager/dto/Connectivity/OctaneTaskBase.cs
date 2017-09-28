using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Connectivity
{
    internal abstract class OctaneTaskBase : IDtoBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        [JsonProperty("serviceId")]
        public Guid? ServiceId { get; set; } = null;

        [JsonProperty("body")]
        public string Body { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
