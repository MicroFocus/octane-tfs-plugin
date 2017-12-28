using log4net;
using log4net.Config;
using MicroFocus.Ci.Tfs.Octane;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Dto.Events;
using MicroFocus.Ci.Tfs.Octane.Tools;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Build.WebApi.Events;
using Microsoft.TeamFoundation.Framework.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Core
{
	public class OctaneTfsPlugin : ISubscriber, IDisposable
	{
		private static string PLUGIN_DISPLAY_NAME = "OctaneTfsPlugin";

		
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static OctaneManagerInitializer _octaneManagerInitializer;

		static OctaneTfsPlugin()
		{
			LogUtils.ConfigureLog4NetForPluginMode();
			_octaneManagerInitializer = OctaneManagerInitializer.GetInstance();
			_octaneManagerInitializer.RunMode = PluginRunMode.ServerPlugin;
			_octaneManagerInitializer.Start();
			Log.Info("OctaneTfsPlugin started");
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
				Log.Info($"ProcessEvent \"{notificationEventArgs.GetType().Name}\" for build {updatedEvent.BuildId}");

				if (_octaneManagerInitializer.IsOctaneInitialized())
				{
					if (notificationEventArgs is BuildStartedEvent)
					{
						CiEvent startedEvent = CiEventUtils.ToCiEvent(build);
						startedEvent.EventType = CiEventType.Started;
						_octaneManagerInitializer.OctaneManager.ReportEventAsync(startedEvent);
					}
					else if (notificationEventArgs is BuildCompletedEvent)
					{
						CiEvent finishEvent = CiEventUtils.ToCiEvent(build);
						finishEvent.EventType = CiEventType.Finished;
						_octaneManagerInitializer.OctaneManager.ReportEventAsync(finishEvent);
					}
				}
				else
				{
					Log.Info($"ProcessEvent cancelled as Octane is not configured.");
				}
			}
			catch (Exception e)
			{
				var msg = $"ProcessEvent \"{notificationEventArgs.GetType().ToString()}\" failed {e.Message}";
				Log.Error(msg, e);
				TeamFoundationApplicationCore.LogException(requestContext, msg, e);
			}
			return EventNotificationStatus.ActionPermitted;
		}

		public void Dispose()
		{
			Octane.RestServer.Server.GetInstance().Stop();
		}
	}
}
