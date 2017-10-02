using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto.parameters;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.dto.pipelines
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
        }
    }
}
