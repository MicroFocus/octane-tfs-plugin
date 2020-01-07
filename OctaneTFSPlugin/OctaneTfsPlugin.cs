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
using log4net;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using MicroFocus.Ci.Tfs.Octane;
using Microsoft.TeamFoundation.Framework.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin
{
    public class OctaneTfsPlugin : ISubscriber
    {
        private static string PLUGIN_DISPLAY_NAME = "OctaneTfsPlugin";

        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static PluginManager _pluginManager;

        static OctaneTfsPlugin()
        {

            LogUtils.WriteWindowsEvent("Plugin loaded", EventLogEntryType.Information);


            LogUtils.ConfigureLog4NetForPluginMode();
            Log.Info("");
            Log.Info("");
            Log.Info("******************************************************************");
            Log.Info("***************OctaneTfsPlugin started****************************");
            Log.Info("******************************************************************");

            _pluginManager = PluginManager.GetInstance();
            _pluginManager.StartPlugin();


            Log.Info("Tfs Version : " + RunModeManager.GetInstance().TfsVersion);

#if Package2019
            Log.Info("Package2019");
#endif

        }

        public Type[] SubscribedTypes()
        {
            var subscribedEventsList = new List<Type>()
            {
#if Package2019
                typeof(Microsoft.TeamFoundation.Build2.Server.BuildStartedEvent),
                typeof(Microsoft.TeamFoundation.Build2.Server.BuildCompletedEvent)
#endif
            };

            return subscribedEventsList.ToArray();
        }

        public string Name => PLUGIN_DISPLAY_NAME;

        public SubscriberPriority Priority => SubscriberPriority.Normal;

        public EventNotificationStatus ProcessEvent(IVssRequestContext requestContext, NotificationType notificationType,
            object notificationEventArgs, out int statusCode, out string statusMessage,
            out Microsoft.TeamFoundation.Common.ExceptionPropertyCollection properties)
        {
            //Log.Info($"ProcessEvent {notificationEventArgs.GetType().Name} - {notificationEventArgs.ToString()}");
            //Log.Info(JsonHelper.SerializeObject(notificationEventArgs, true));
            statusCode = 0;
            properties = null;
            statusMessage = String.Empty;
            try
            {
#if Package2019
                if (notificationEventArgs is Microsoft.TeamFoundation.Build2.Server.BuildEventBase)
                {
                    Microsoft.TeamFoundation.Build2.Server.BuildData build = (Microsoft.TeamFoundation.Build2.Server.BuildData)((Microsoft.TeamFoundation.Build2.Server.BuildEventBase)notificationEventArgs).Build;
                    Log.Info($"ProcessEvent {notificationEventArgs.GetType().Name} for project {build.ProjectId}, status: {build.Status}, BuildNumber: {build.BuildNumber}, DefinitionName: {build.Definition.Name}, DefinitionId: {build.Definition.Id}");
                    CiEvent ciEvent = ConvertToCiEvent2019(build);
                    if (notificationEventArgs is Microsoft.TeamFoundation.Build2.Server.BuildStartedEvent)
                    {
                        ciEvent.EventType = CiEventType.Started;
                        _pluginManager.GeneralEventsQueue.Add(ciEvent);
                    }
                    else if (notificationEventArgs is Microsoft.TeamFoundation.Build2.Server.BuildCompletedEvent)
                    {
                        ciEvent.EventType = CiEventType.Finished;
                        _pluginManager.HandleFinishEvent(ciEvent);
                    }
                }
#endif
            }
            catch (Exception e)
            {
                var msg = $"ProcessEvent {notificationEventArgs.GetType().Name} failed {e.Message}";
                Log.Error(msg, e);
            }
            return EventNotificationStatus.ActionPermitted;
        }


#if Package2019
        private static CiEvent ConvertToCiEvent2019(Microsoft.TeamFoundation.Build2.Server.BuildData build)
        {
            TfsBuildInfo buildInfo = new TfsBuildInfo(build.Id.ToString(), build.BuildNumber, build.ProjectId.ToString(), build.Definition.Id.ToString());

            bool isManualCause = Microsoft.TeamFoundation.Build2.Server.BuildReason.Manual.Equals(build.Reason);
            var ciEvent = createEvent(buildInfo, build.Definition.Name, isManualCause, build.StartTime, build.FinishTime);

            if (build.Result.HasValue)
            {
                switch (build.Result)
                {
                    case Microsoft.TeamFoundation.Build2.Server.BuildResult.Succeeded:
                        ciEvent.BuildResult = CiBuildResult.Success;
                        break;
                    case Microsoft.TeamFoundation.Build2.Server.BuildResult.Failed:
                        ciEvent.BuildResult = CiBuildResult.Failure;
                        break;
                    case Microsoft.TeamFoundation.Build2.Server.BuildResult.Canceled:
                        ciEvent.BuildResult = CiBuildResult.Aborted;
                        break;
                    case Microsoft.TeamFoundation.Build2.Server.BuildResult.PartiallySucceeded:
                        ciEvent.BuildResult = CiBuildResult.Unstable;
                        break;
                    default:
                        ciEvent.BuildResult = CiBuildResult.Unavailable;
                        break;
                }
            }

            return ciEvent;
        }
#endif

        private static CiEvent createEvent(TfsBuildInfo buildInfo, String buildDefinitionName, bool isManualCause, DateTime? startTime,
            DateTime? finishTime)
        {
            var ciEvent = new CiEvent();
            ciEvent.BuildInfo = buildInfo;

            ciEvent.BuildId = buildInfo.BuildId + "." + buildInfo.BuildNumber;
            ciEvent.BuildTitle = buildInfo.BuildNumber;

            ciEvent.ProjectDisplayName = buildDefinitionName;
            ciEvent.PhaseType = "post";

            //cause
            var cause = new CiEventCause();
            cause.CauseType = isManualCause ? CiEventCauseType.User : CiEventCauseType.Undefined;
            ciEvent.Causes.Add(cause);

            if (startTime.HasValue)
            {
                ciEvent.StartTime = OctaneUtils.ConvertToOctaneTime(startTime.Value);
                if (finishTime.HasValue)
                {
                    ciEvent.Duration = (long)(finishTime.Value - startTime.Value).TotalMilliseconds;
                }
            }

            return ciEvent;
        }
    }
}
