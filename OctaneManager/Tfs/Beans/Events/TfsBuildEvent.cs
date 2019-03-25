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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.v1;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity;
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
            var elements = Resource.Definition.Url.Split('/').ToList();
            var i = elements.FindIndex(x => x == "_apis");

            ciEvent.BuildInfo = new TfsBuildInfo(Resource.Id.ToString(), Resource.BuildNumber, elements[i - 1], Resource.Definition.Id);

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

            ciEvent.BuildId = ciEvent.BuildInfo.BuildId + "." + ciEvent.BuildInfo.BuildNumber;
            //ciEvent.Project = OctaneUtils.GenerateOctaneJobCiId(ciEvent.BuildInfo.ProjectId, ciEvent.BuildInfo.BuildDefinitionId);
            ciEvent.BuildTitle = ciEvent.BuildInfo.BuildNumber;
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
