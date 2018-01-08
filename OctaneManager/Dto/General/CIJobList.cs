using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.dto.pipelines;
using Newtonsoft.Json;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.dto.general
{
    internal class CiJobList : IDtoBase
    {
        [JsonProperty("jobs")]
        public IList<PipelineNode> Jobs { get; set; }

        
        public CiJobList()
        {
            Jobs = new List<PipelineNode>();
        }

        public PipelineNode this[string key] 
        {
            get { return Jobs.FirstOrDefault(x => x.JobCiId == key); }
            set
            {
                var job = Jobs.FirstOrDefault(x => x.JobCiId == key);
                Jobs[Jobs.IndexOf(job)] = value;
            }
        }        
    }
}
