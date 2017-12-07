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
		public CiEvent() { }

		public CiEvent Clone()
		{
			CiEvent clonedEvent = new CiEvent();
			clonedEvent.ProjectDisplayName = ProjectDisplayName;
			clonedEvent.EventType = new CiEventType(EventType.ToString());
			clonedEvent.BuildCiId = BuildCiId;
			clonedEvent.Project = Project;
			clonedEvent.Number = Number;
			clonedEvent.BuildResult = BuildResult;
			clonedEvent.StartTime = StartTime;
			clonedEvent.EstimatedDuration = EstimatedDuration;
			clonedEvent.Duration = Duration;
			clonedEvent.ScmData = ScmData; //TODO: [URGENT] Refactor with copy constructor!!!
			clonedEvent.PhaseType = PhaseType;
			return clonedEvent;
		}

		public string ProjectDisplayName { get; set; }

        [JsonConverter(typeof(ToStringJsonConverter))]
        public CiEventType EventType { get; set; }

        public string BuildCiId { get; set; }

        public string Project { get; set; }

        public string Number { get; set; }

        public List<CiEventCause> Causes => new List<CiEventCause>();

        public List<CiParameter> Parameters => new List<CiParameter>();

        [JsonProperty("result")]
        public CiBuildResult BuildResult { get; set; }

        public long StartTime { get; set; }

        public long EstimatedDuration { get; set; }

        public long Duration { get; set; }

        public ScmData ScmData
        {
            get;
            set;
        }

        public string PhaseType { get; set; }
    }
}
