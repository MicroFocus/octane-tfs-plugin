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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Parameters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Pipelines
{
	internal class PipelineNode : IDtoBase
    {
        [JsonProperty("jobCiId")]
        public string JobCiId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("parameters")]
        public IList<CiParameter> Parameters { get; set; }
        [JsonProperty("phaseInternal")]
        public IList<PipelinePhase> PhasesInternal { get; set; }
        [JsonProperty("phasePostBuild")]
        public IList<PipelinePhase> PhasesPostBuild { get; set; }

        public PipelineNode() { }

        public PipelineNode(string jobCiId, string name)
        {
            JobCiId = jobCiId;
            Name = name;
            Parameters = new List<CiParameter>();
            PhasesInternal = new List<PipelinePhase>();
            PhasesPostBuild = new List<PipelinePhase>();
        }
    }
}
