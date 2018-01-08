using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.dto;
using Newtonsoft.Json;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Scm
{
	public  class ScmCommitFileChange : IDtoBase
    {
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("file")]
		public string File { get; set; }
	}
}
