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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.v1.SCM;
using Newtonsoft.Json;
using System;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.ApiItems
{
	public class TfsBuild
	{
		public int Id { get; set; }
		[JsonProperty("buildNumber")]
		public string Name { get; set; }
		public string Uri { get; set; }
		public string Url { get; set; }
		public string Status { get; set; }
		public string Result { get; set; }
		public string StartTime { get; set; }
		public string FinishTime { get; set; }
		public string SourceBranch { get; set; }
		public string SourceVersion { get; set; }
		public TfsScmRepository Repository { get; set; }
		public override string ToString()
		{
			return $"{Id}-{Name}";
		}
	}
}
