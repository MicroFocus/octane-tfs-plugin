/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using Microsoft.TeamFoundation.Build.WebApi;
using System.Linq;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin
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

			ciEvent.Project = OctaneUtils.GenerateOctaneJobCiId(buildInfo.CollectionName, buildInfo.Project, buildInfo.BuildDefinitionId);
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
				ciEvent.StartTime = OctaneUtils.ConvertToOctaneTime(build.StartTime.Value);
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
