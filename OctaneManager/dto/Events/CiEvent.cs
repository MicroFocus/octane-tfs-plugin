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
using MicroFocus.Ci.Tfs.Octane.Tools;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Dto
{
    public class CiEvent : IDtoBase
    {
        [JsonProperty("projectDisplayName")]
        public string ProjectDisplayName { get; set; }

        [JsonProperty("eventType")]
        [JsonConverter(typeof(ToStringJsonConverter))]
        public CiEventType EventType { get; set; }

        [JsonProperty("buildCiId")]
        public string BuildCiId { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("causes")]
        public List<CiEventCause> Causes => new List<CiEventCause>();

        [JsonProperty("parameters")]

        public List<CiParameter> Parameters => new List<CiParameter>();

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

        [JsonProperty("phaseType")]
        public string PhaseType { get; set; }

        public CiEvent() {  }
        public CiEvent(CiEvent other)
        {
            ProjectDisplayName = other.ProjectDisplayName;
            EventType = new CiEventType(other.EventType.ToString());
            BuildCiId = other.BuildCiId;
            Project = other.Project;
            Number = other.Number;
            BuildResult = other.BuildResult;
            StartTime = other.StartTime;
            EstimatedDuration = other.EstimatedDuration;
            Duration = other.Duration;
            ScmInfo = other.ScmInfo; //TODO: [URGENT] Refactor with copy constructor!!!
            PhaseType = other.PhaseType;
        }
    }
}
