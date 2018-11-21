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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using MicroFocus.Ci.Tfs.Octane;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Build.WebApi.Events;
using Microsoft.TeamFoundation.Framework.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

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


            LogUtils.ConfigureLog4NetForPluginMode(false);
			Log.Info("");
			Log.Info("");
			Log.Info("******************************************************************");
			Log.Info("***************OctaneTfsPlugin started****************************");
			Log.Info("******************************************************************");

			_pluginManager = PluginManager.GetInstance();
			_pluginManager.StartPlugin();
		}

		public Type[] SubscribedTypes()
		{
			var subscribedEventsList = new List<Type>()
			{
				typeof(BuildCompletedEvent),
				typeof(BuildStartedEvent)
			};

			return subscribedEventsList.ToArray();
		}

		public string Name => PLUGIN_DISPLAY_NAME;

		public SubscriberPriority Priority => SubscriberPriority.Normal;

        public EventNotificationStatus ProcessEvent(IVssRequestContext requestContext, NotificationType notificationType,
			object notificationEventArgs, out int statusCode, out string statusMessage,
			out Microsoft.TeamFoundation.Common.ExceptionPropertyCollection properties)
		{
			statusCode = 0;
			properties = null;
			statusMessage = String.Empty;
			try
			{
				BuildUpdatedEvent updatedEvent = (BuildUpdatedEvent)notificationEventArgs;
				Build build = updatedEvent.Build;
				Log.Info($"ProcessEvent {notificationEventArgs.GetType().Name} for build {updatedEvent.BuildId} (Build Number : {updatedEvent.Build.BuildNumber}, Build Definition: {updatedEvent.Build.Definition.Name})");

				CiEvent ciEvent = CiEventUtils.ToCiEvent(build);
				if (notificationEventArgs is BuildStartedEvent)
				{
					ciEvent.EventType = CiEventType.Started;
					_pluginManager.GeneralEventsQueue.Add(ciEvent);
				}
				else if (notificationEventArgs is BuildCompletedEvent)
				{
					ciEvent.EventType = CiEventType.Finished;
					_pluginManager.HandleFinishEvent(ciEvent);
				}
			}
			catch (Exception e)
			{
				var msg = $"ProcessEvent {notificationEventArgs.GetType().Name} failed {e.Message}";
				Log.Error(msg, e);
				TeamFoundationApplicationCore.LogException(requestContext, msg, e);
			}
			return EventNotificationStatus.ActionPermitted;
		}
	}
}
