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
using System.Reflection;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin
{
	public class OctaneTfsPlugin : ISubscriber, IDisposable
	{
		private static string PLUGIN_DISPLAY_NAME = "OctaneTfsPlugin";

		
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static PluginManager _pluginManager;
		private static int instanceCount;

		static OctaneTfsPlugin()
		{
			LogUtils.ConfigureLog4NetForPluginMode();
			_pluginManager = PluginManager.GetInstance();
			_pluginManager.StartPlugin();
			Log.Info("******************************************************************");
			Log.Info("***************OctaneTfsPlugin started****************************");
			Log.Info("******************************************************************");
		}

		public OctaneTfsPlugin()
		{
			instanceCount++;
			Log.Debug($"OctaneTfsPlugin ctor, new instance count {instanceCount}");
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

				if (_pluginManager.IsInitialized())
				{
					if (notificationEventArgs is BuildStartedEvent)
					{
						CiEvent startedEvent = CiEventUtils.ToCiEvent(build);
						startedEvent.EventType = CiEventType.Started;
						_pluginManager.EventManager.ReportEventAsync(startedEvent);
					}
					else if (notificationEventArgs is BuildCompletedEvent)
					{
						CiEvent finishEvent = CiEventUtils.ToCiEvent(build);
						finishEvent.EventType = CiEventType.Finished;
						_pluginManager.EventManager.ReportEventAsync(finishEvent);
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
			instanceCount--;
			Log.Debug($"OctaneTfsPlugin disposing, new instance count {instanceCount}");
			if (instanceCount == 0)
			{
				//_octaneManagerInitializer.Stop();
			}
		}
	}
}
