using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.Dto.General;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Events
{
    public class CiEventsList
    {
        [JsonProperty("server")]
        public CiServerInfo Server { get; set; }

        [JsonProperty("events")]
        public List<CiEvent> Events { get; set; }

        public CiEventsList() => Events = new List<CiEvent>();
    }
}
