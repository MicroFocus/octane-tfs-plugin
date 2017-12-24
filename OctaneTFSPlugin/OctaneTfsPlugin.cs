using MicroFocus.Ci.Tfs.Octane;
using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Dto.Events;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Build.WebApi.Events;
using Microsoft.TeamFoundation.Framework.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFile = @"agent-log-config.xml", Watch = true)]
namespace MicroFocus.Ci.Tfs.Core
{
	public class OctaneTfsPlugin : ISubscriber
	{
		private static string PLUGIN_DISPLAY_NAME = "OctaneTfsPlugin";

		private static readonly TimeSpan _initTimeout = new TimeSpan(0, 0, 0, 5);

		private static OctaneManager _octaneManager = null;

		private static Task _octaneInitializationThread = null;
		private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		static OctaneTfsPlugin()
		{
			if (_octaneInitializationThread == null)
			{
				_octaneInitializationThread =
					Task.Factory.StartNew(() => InitializeOctaneManager(_cancellationTokenSource.Token),
						TaskCreationOptions.LongRunning);
			}
		}


		private static void InitializeOctaneManager(CancellationToken token)
		{
			while (!IsOctaneInitialized())
			{
				if (token.IsCancellationRequested)
				{
					TeamFoundationApplicationCore.Log("Octane initialization thread was requested to quit!", 1, EventLogEntryType.Information);
					break;
				}
				try
				{
					_octaneManager = new OctaneManager();
					_octaneManager.Init();

				}
				catch (Exception ex)
				{
					TeamFoundationApplicationCore.Log($"Error initializing octane plugin! {ex.Message}", 1, EventLogEntryType.Error);
				}

				//Sleep till next retry
				Thread.Sleep(_initTimeout);

			}
		}

		private static bool IsOctaneInitialized()
		{
			return _octaneManager != null && _octaneManager.IsInitialized;
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
				Trace.WriteLine($"ProcessEvent \"{notificationEventArgs.GetType().ToString()}\" for build {updatedEvent.BuildId}");

				if (IsOctaneInitialized())
				{
					if (notificationEventArgs is BuildStartedEvent)
					{
						CiEvent startedEvent = CiEventUtils.ToCiEvent(build);
						startedEvent.EventType = CiEventType.Started;
						_octaneManager.ReportEventAsync(startedEvent);
					}
					else if (notificationEventArgs is BuildCompletedEvent)
					{
						CiEvent finishEvent = CiEventUtils.ToCiEvent(build);
						finishEvent.EventType = CiEventType.Finished;
						_octaneManager.ReportEventAsync(finishEvent);
					}
				}
				else
				{
					Trace.WriteLine($"ProcessEvent cancelled as Octane is not configured.");
				}
			}
			catch (Exception e)
			{

				Trace.TraceError($"An error \"{e.Message}\" occured during processing of notification.", e);

				TeamFoundationApplicationCore.Log(requestContext, "HPE  : Process Server Event",
					$"The error occured during processing notification: {e}", 123, EventLogEntryType.Error);
			}
			return EventNotificationStatus.ActionPermitted;
		}



	}
}
