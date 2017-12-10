using Newtonsoft.Json;
using System.Collections.Generic;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1.SCM
{
	public class TfsScmCommit
	{
		public string TreeId { get; set; }
		public string CommitId { get; set; }
		public string Comment { get; set; }
		public List<string> Parents { get; set; }
		public string RemoteUrl { get; set; }
		[JsonProperty("_links")]
		public TfsScmCommitLinks Links { get; set; }
		public List<TfsScmCommitChange> Changes { get; set; }
		public TfsScmCommitAuthor Author { get; set; }
		public TfsScmCommitAuthor Committer { get; set; }
		
		public override string ToString()
		{
			return CommitId + ":" + Comment;
		}
	}

	public class TfsScmCommitLinks
	{
		public TfsLink Repository { get; set; }
		public TfsLink Changes { get; set; }
		public TfsLink Web { get; set; }

	}
}
