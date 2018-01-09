using Newtonsoft.Json;
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Scm
{
	public class ScmCommit : IDtoBase
	{
		[JsonProperty("time")]
		public long Time { get; set; }
		[JsonProperty("user")]
		public string User { get; set; }
		[JsonProperty("userEmail")]
		public string UserEmail { get; set; }
		[JsonProperty("revId")]
		public string RevId { get; set; }
		[JsonProperty("parentRevId")]
		public string ParentRevId { get; set; }
		[JsonProperty("comment")]
		public string Comment { get; set; }
		[JsonProperty("changes")]
		public List<ScmCommitFileChange> Changes{ get; set; }


	}
}
