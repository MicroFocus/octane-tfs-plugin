using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.dto.parameters;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.General;
using Newtonsoft.Json;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.dto.pipelines
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
