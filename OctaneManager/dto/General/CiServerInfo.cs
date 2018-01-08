using System;
using Newtonsoft.Json;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.General
{
    public class CiServerInfo
    {
        [JsonProperty("type")]
        public string Type => "TFS";
        [JsonProperty("version")]
        public string Version => "2017"; //TODO : Get programaticly!
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }

        [JsonProperty("instanceIdFrom")]
        public long InstanceIdFrom { get; set; }

        [JsonProperty("sendingTime")]
        public long SendingTime { get; set; }


    }
}
