using Newtonsoft.Json;
using System;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Connectivity
{
	public class OctaneTaskResult : OctaneTaskBase
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
