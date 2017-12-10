using MicroFocus.Ci.Tfs.Octane.dto;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Scm
{
    public class ScmRepository : IDtoBase
	{
		[JsonProperty("type")]
		public string Type { get; set; }
		[JsonProperty("url")]
		public string Url { get; set; }
		[JsonProperty("branch")]
		public string Branch { get; set; }
	}
}
