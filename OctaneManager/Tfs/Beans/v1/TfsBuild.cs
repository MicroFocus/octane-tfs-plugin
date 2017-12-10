using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1.SCM;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems
{
    public class TfsBuild
    {
        public int Id { get; set; }
        [JsonProperty("buildNumber")]
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
		public string  SourceBranch { get; set; }
		public string  SourceVersion { get; set; }
		public TfsScmRepository Repository { get; set; }
	}
}
