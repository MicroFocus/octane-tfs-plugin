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
