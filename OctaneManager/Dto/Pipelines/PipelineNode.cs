using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto.parameters;

namespace MicroFocus.Ci.Tfs.Octane.dto.pipelines
{
    internal class PipelineNode
    {
        public string JobCiId { get; set; }

        public string Name { get; set; }

        public IList<CiParameter> Parameters { get; set; }

        public IList<PipelinePhase> PhasesInternal { get; set; }

        public IList<PipelinePhase> PhasesPostBuild { get; set; }

        public PipelineNode() { }

        public PipelineNode(string jobCiId, string name)
        {
            JobCiId = jobCiId;
            Name = name;
        }
    }
}
