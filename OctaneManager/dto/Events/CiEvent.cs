using MicroFocus.Ci.Tfs.Octane.dto;
using MicroFocus.Ci.Tfs.Octane.dto.parameters;
using MicroFocus.Ci.Tfs.Octane.Dto.Events;
using MicroFocus.Ci.Tfs.Octane.Dto.Scm;
using MicroFocus.Ci.Tfs.Octane.Tools;
using Newtonsoft.Json;
using System.Collections.Generic;

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
		[JsonConverter(typeof(ToStringJsonConverter))]
		public CiBuildResult BuildResult { get; set; }

		[JsonProperty("startTime")]
		public long StartTime { get; set; }

		[JsonProperty("estimatedDuration")]
		public long EstimatedDuration { get; set; }

		[JsonProperty("duration")]
		public long Duration { get; set; }

		[JsonProperty("scmData")]
		public ScmData ScmData { get; set; }

		[JsonProperty("phaseType")]
		public string PhaseType { get; set; }
	}
}
