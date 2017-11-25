using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Dto.General
{
    public class CiServerInfo
    {
        [JsonProperty("type")]
        public string Type => "tfs";
        [JsonProperty("version")]
        public string Version => "2017"; //TODO : Get programaticly!
        [JsonProperty("url")]
        public Uri Url { get; set; }
        [JsonProperty("instanceId")]
        public Guid InstanceId { get; set; }

        [JsonProperty("instanceIdFrom")]
        public long InstanceIdFrom { get; set; }

        [JsonProperty("sendingTime")]
        public long SendingTime { get; set; }


    }
}
