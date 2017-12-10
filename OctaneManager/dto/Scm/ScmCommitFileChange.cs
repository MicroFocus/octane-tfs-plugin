using MicroFocus.Ci.Tfs.Octane.dto;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Scm
{
	public  class ScmCommitFileChange : IDtoBase
    {
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("file")]
		public string File { get; set; }
	}
}
