using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto.pipelines;

namespace MicroFocus.Ci.Tfs.Octane.dto.general
{
    internal class CiJobList : IDtoBase
    {
        public IList<PipelineNode> Jobs { get; set; }

        public CiJobList()
        {
            Jobs = new List<PipelineNode>();
        }
    }
}
