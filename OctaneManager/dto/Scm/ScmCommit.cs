
using System.Collections.Generic;
using MicroFocus.Ci.Tfs.Octane.dto;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Scm
{
	public class ScmCommit : IDtoBase
	{
		public long Time { get; set; }
		public string User { get; set; }
		public string UserEmail { get; set; }
		public string RevId { get; set; }
		public string ParentRevId { get; set; }
		public string Comment { get; set; }
		public List<ScmCommitFileChange> Changes{ get; set; }


	}
}
