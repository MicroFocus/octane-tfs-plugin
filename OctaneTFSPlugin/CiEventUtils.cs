using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Dto.Events;
using MicroFocus.Ci.Tfs.Octane.Tools;
using Microsoft.TeamFoundation.Build.WebApi;
using System.Linq;


namespace MicroFocus.Ci.Tfs.Core
{
	public static class CiEventUtils
	{
		public static CiEvent ToCiEvent(Build build)
		{
			var ciEvent = new CiEvent();


			//create  build info
			TfsBuildInfo buildInfo = new TfsBuildInfo();
			var elements = build.Definition.Url.Split('/').ToList();
			var i = elements.FindIndex(x => x == "_apis");
			buildInfo.CollectionName = elements[i - 2];
			buildInfo.Project = elements[i - 1];
			buildInfo.BuildDefinitionId = build.Definition.Id.ToString();
			buildInfo.BuildId = build.Id.ToString();
			buildInfo.BuildName = build.BuildNumber;
			ciEvent.BuildInfo = buildInfo;

			//start filling ciEvent
			ciEvent.BuildId = buildInfo.BuildId + "." + buildInfo.BuildName;

			ciEvent.Project = TestResultUtils.GenerateOctaneJobCiId(buildInfo.CollectionName, buildInfo.Project, buildInfo.BuildDefinitionId);
			ciEvent.BuildTitle = buildInfo.BuildName;
			var cause = new CiEventCause();
			switch (build.Reason)
			{
				case BuildReason.Manual:
					cause.CauseType = CiEventCauseType.User;
					break;
				default:
					cause.CauseType = CiEventCauseType.Undefined;
					break;

			}
			ciEvent.Causes.Add(cause);
			
			ciEvent.ProjectDisplayName = build.Definition.Name;
			ciEvent.PhaseType = "post";


			if (build.StartTime.HasValue)
			{
				ciEvent.StartTime = TestResultUtils.ConvertToOctaneTime(build.StartTime.Value);
				if (build.FinishTime.HasValue)
				{
					ciEvent.Duration = (long)(build.FinishTime.Value - build.StartTime.Value).TotalMilliseconds;
				}
			}
			
			if (build.Result.HasValue)
			{
				switch (build.Result)
				{
					case BuildResult.Succeeded:
						ciEvent.BuildResult = CiBuildResult.Success;
						break;
					case BuildResult.Failed:
						ciEvent.BuildResult = CiBuildResult.Failure;
						break;
					case BuildResult.Canceled:
						ciEvent.BuildResult = CiBuildResult.Aborted;
						break;
					case BuildResult.PartiallySucceeded:
						ciEvent.BuildResult = CiBuildResult.Unstable;
						break;
					default:
						ciEvent.BuildResult = CiBuildResult.Unavailable;
						break;
				}

			}


			return ciEvent;
		}
	}
}
