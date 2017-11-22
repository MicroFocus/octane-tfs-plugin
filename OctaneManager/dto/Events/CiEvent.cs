using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto;
using MicroFocus.Ci.Tfs.Octane.dto.parameters;
using MicroFocus.Ci.Tfs.Octane.Dto.Events;
using MicroFocus.Ci.Tfs.Octane.Dto.Scm;
using MicroFocus.Ci.Tfs.Octane.Dto.Snapshots;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Dto
{
    class CiEvent : IDtoBase
    {
        [JsonProperty("projectDisplayName")]
        public string ProjectDisplayName { get; set; }

        [JsonProperty("eventType")]
        public CiEventType EventType { get; set; }

        [JsonProperty("buildCiId")]
        public string BuildCiId { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("causes")]
        public List<CiEventCause> Causes { get; set; }

        [JsonProperty("parameters")]
        public List<CiParameter> Parameters { get; set; }

        [JsonProperty("result")]
        public CiBuildResult BuildResult { get; set; }

        [JsonProperty("startTime")]
        public long StartTime { get; set; }

        [JsonProperty("estimatedDuration")]
        public long EstimatedDuration { get; set; }

        [JsonProperty("duration")]    
        public long Duration { get; set; }

        [JsonProperty("scmData")]
        public ScmData ScmInfo
        {
            get;
            set;
        }


             


    }
}
