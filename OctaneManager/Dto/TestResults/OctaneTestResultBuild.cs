/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.TestResults
{
	// <build server_id="59fd04f6-041d-43a0-984a-ab27e3edf3bb" job_id="mavenTest" job_name="mavenTest" build_id="6" build_name="6" />
	public class OctaneTestResultBuild
	{
		[XmlAttribute("server_id")]
		public string ServerId { get; set; }

		[XmlAttribute("job_id")]
		public string JobId { get; set; }

		[XmlAttribute("build_id")]
		public string BuildId { get; set; }

		public static OctaneTestResultBuild Create(string serverId, string buildId, string jobId)
		{
			OctaneTestResultBuild build = new OctaneTestResultBuild();
			build.ServerId = serverId;
			build.BuildId = buildId;
			build.JobId = jobId;
			return build;
		}
	}
}
