using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.dto.parameters;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Scm;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto
{
	public class CiEvent : IDtoBase
	{
		public CiEvent() { }

		public CiEvent Clone()
		{
			CiEvent clonedEvent = new CiEvent();
			clonedEvent.ProjectDisplayName = ProjectDisplayName;
			clonedEvent.EventType = new CiEventType(EventType.ToString());
			clonedEvent.BuildId = BuildId;
			clonedEvent.Project = Project;
			clonedEvent.BuildTitle = BuildTitle;
			clonedEvent.BuildResult = BuildResult;
			clonedEvent.StartTime = StartTime;
			clonedEvent.EstimatedDuration = EstimatedDuration;
			clonedEvent.Duration = Duration;
			clonedEvent.ScmData = ScmData; 
			clonedEvent.PhaseType = PhaseType;
			clonedEvent.BuildInfo = BuildInfo;
			return clonedEvent;
		}

		[JsonProperty("projectDisplayName")]
		public string ProjectDisplayName { get; set; }

		[JsonProperty("eventType")]
		[JsonConverter(typeof(ToStringJsonConverter))]
		public CiEventType EventType { get; set; }

		[JsonProperty("buildCiId")]
		public string BuildId { get; set; }

		[JsonProperty("number")]
		public string BuildTitle { get; set; }

		[JsonProperty("project")]
		public string Project { get; set; }

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

		[JsonIgnore]
		public TfsBuildInfo BuildInfo { get; set; }
	}
}
