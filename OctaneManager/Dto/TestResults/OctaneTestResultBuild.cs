using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroFocus.Ci.Tfs.Octane.Dto.TestResults
{
    // <build server_id="59fd04f6-041d-43a0-984a-ab27e3edf3bb" job_id="mavenTest" job_name="mavenTest" build_id="6" build_name="6" />
    public class OctaneTestResultBuild
    {
        [XmlAttribute("server_id")]
        public string ServerId { get; set; }

        [XmlAttribute("job_id")]
        public string JobId { get; set; }

        [XmlAttribute("job_name")]
        public string JobName { get; set; }

        [XmlAttribute("build_id")]
        public int BuildId { get; set; }

        [XmlAttribute("build_name")]
        public string BuildName { get; set; }

        public static OctaneTestResultBuild Create(string serverId, int buildId, string buildName, string jobId, string jobName)
        {
            OctaneTestResultBuild build = new OctaneTestResultBuild();
            build.ServerId = serverId;
            build.BuildId = buildId;
            build.BuildName = buildName;
            build.JobId = jobId;
            build.JobName = jobName;
            return build;
        }
    }
}
