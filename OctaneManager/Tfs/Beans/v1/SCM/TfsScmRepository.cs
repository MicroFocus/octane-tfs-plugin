
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1.SCM
{
	public class TfsScmRepository
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string DefaultBranch { get; set; }
		public string RemoteUrl { get; set; }
		public string Url { get; set; }
		
	}
}
