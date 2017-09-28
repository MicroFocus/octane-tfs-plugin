using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Connectivity
{
    internal class OctaneTaskResult : OctaneTaskBase
    {
        [JsonProperty("status")]        
        public int Status { get; set; }

        public OctaneTaskResult() { }

        public OctaneTaskResult(int status, Guid id,string body)
        {
            Headers.Add("Content-Type","application/json");
            Body = body;
            Id = id;
            Status = status;
        }

    }
}
