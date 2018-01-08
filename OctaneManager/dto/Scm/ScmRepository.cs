using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.dto;
using Newtonsoft.Json;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Scm
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
