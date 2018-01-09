using Newtonsoft.Json;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Parameters
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
