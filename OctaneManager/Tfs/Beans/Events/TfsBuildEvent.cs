using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.v1;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System;
using System.Linq;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.Events
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
			return JsonHelper.SerializeObject(this);
		}

		public CiEvent ToCiEvent()
		{
			var ciEvent = new CiEvent();

			//create  build info
			TfsBuildInfo buildInfo = new TfsBuildInfo();
			var elements = Resource.Definition.Url.Split('/').ToList();
			var i = elements.FindIndex(x => x == "_apis");
			buildInfo.CollectionName = elements[i - 2];
			buildInfo.Project = elements[i - 1];
			buildInfo.BuildDefinitionId = Resource.Definition.Id;
			buildInfo.BuildId = Resource.Id.ToString();
			buildInfo.BuildName = Resource.BuildNumber;
			ciEvent.BuildInfo = buildInfo;

			//start filling ciEvent
			switch (EventType)
			{
				case "build.complete":
					ciEvent.EventType = CiEventType.Finished;
					break;
				default:
					ciEvent.EventType = CiEventType.Undefined;
					break;
			}

			ciEvent.BuildId = buildInfo.BuildId + "." + buildInfo.BuildName;
			ciEvent.Project = OctaneUtils.GenerateOctaneJobCiId(buildInfo.CollectionName, buildInfo.Project, buildInfo.BuildDefinitionId);
			ciEvent.BuildTitle = buildInfo.BuildName;
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
			ciEvent.StartTime = OctaneUtils.ConvertToOctaneTime(Resource.StartTime);
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
			}

			return ciEvent;
		}
	}
}
