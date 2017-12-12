using MicroFocus.Ci.Tfs.Octane.dto.pipelines;
using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Dto.Events;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.Events
{
	public class TfsBuildEvent
	{

		public Guid Id { get; set; }
		public string EventType { get; set; }

		public BuildEventResource Resource { get; set; }
		public class BuildEventResource
		{
			public Uri Uri { get; set; }
			public int Id { get; set; }
			public string BuildNumber { get; set; }
			public Uri Url { get; set; }
			public DateTime StartTime { get; set; }
			public DateTime FinishTime { get; set; }
			public string Reason { get; set; }
			public string Status { get; set; }
			public TfsBuildDefinition Definition { get; set; }

		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		public CiEvent ToCiEvent()
		{
			var ciEvent = new CiEvent();
			switch (EventType)
			{
				case "build.complete":
					ciEvent.EventType = CiEventType.Finished;
					break;
				default:
					ciEvent.EventType = CiEventType.Undefined;
					break;
			}


			ciEvent.BuildCiId = Resource.BuildNumber.ToString();
			ciEvent.Project = GenerateOctaneProjectIdFromBuildDefinitionUrl(Resource.Definition.Url.ToString());
			ciEvent.Number = Resource.Id.ToString();
			var cause = new CiEventCause();
			switch (Resource.Reason)
			{
				case "manual":
					cause.CauseType = CiEventCauseType.User;
					break;
				default:
					cause.CauseType = CiEventCauseType.Undefined;
					break;

			}
			ciEvent.Causes.Add(cause);
			ciEvent.StartTime = TestResultUtils.ConvertToOctaneTime(Resource.StartTime);
			ciEvent.Duration = (long)(Resource.FinishTime - Resource.StartTime).TotalMilliseconds;
			ciEvent.ProjectDisplayName = Resource.Definition.Name;
			ciEvent.PhaseType = "post";

			switch (Resource.Status)
			{
				case "succeeded":
					ciEvent.BuildResult = CiBuildResult.Success;
					break;
				case "failed":
					ciEvent.BuildResult = CiBuildResult.Failure;
					break;
				case "stopped":
					ciEvent.BuildResult = CiBuildResult.Aborted;
					break;
				default:
					ciEvent.BuildResult = CiBuildResult.Unavailable;
					break;


					//UNSTABLE("unstable"),
					//ABORTED("aborted"),
			}

			return ciEvent;
		}

		//Generate project id for octane : CollectionName.projectId.buildDefinitionId
		private string GenerateOctaneProjectIdFromBuildDefinitionUrl(string value)
		{
			var elements = value.Split('/').ToList();

			var i = elements.FindIndex(x => x == "_apis");

			var id = PipelineNode.GenerateOctaneJobCiId(elements[i - 2], elements[i - 1], Resource.Definition.Id);
			return id;
		}
	}
}
