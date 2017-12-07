
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1.SCM
{
	public class TfsScmCommitChange
	{
		public string ChangeType { get; set; }
		public TfsScmCommitChangeItem Item { get; set; }

	}

	public class TfsScmCommitChangeItem
	{
		public string path { get; set; }
		public bool isFolder { get; set; }

	}
}
