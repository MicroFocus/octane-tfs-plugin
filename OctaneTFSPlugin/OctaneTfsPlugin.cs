using log4net;
using log4net.Config;
using MicroFocus.Ci.Tfs.Octane;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Dto.Events;
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

		private static readonly TimeSpan _initTimeout = new TimeSpan(0, 0, 0, 30);

		private static OctaneManager _octaneManager = null;

		private static Task _octaneInitializationThread = null;
		private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		static OctaneTfsPlugin()
		{
			ConfigureLog4Net();

			Log.Info("OctaneTfsPlugin started");
			if (_octaneInitializationThread == null)
			{
				_octaneInitializationThread =
					Task.Factory.StartNew(() => InitializeOctaneManager(_cancellationTokenSource.Token),
						TaskCreationOptions.LongRunning);
			}

			Octane.RestServer.Server.GetInstance().Start();
		}

		private static void ConfigureLog4Net()
		{
			//find config file to load
			var logConfigFileName = "OctaneTFSPluginLogConfig.xml";
			string fullPath = null;
			if (File.Exists(logConfigFileName)) //check on C://windows/system32
			{
				fullPath = logConfigFileName;
			}
			else
			{
				var dllPath = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath;
				string temp = Path.Combine(Path.GetDirectoryName(dllPath), logConfigFileName);
				if (File.Exists(temp))
				{
					fullPath = temp;
				}
			}
			if (fullPath != null)
			{
				Log.Debug($"Log4net configuration file {fullPath}");
				XmlConfigurator.ConfigureAndWatch(new FileInfo(fullPath));

				//change path to log file for unrooted files (like 'abc.log')
				var logFolder = Path.Combine(ConfigurationManager.ConfigFolder, "Logs");
				log4net.Repository.ILoggerRepository repository = LogManager.GetRepository();
				foreach (log4net.Appender.IAppender appender in repository.GetAppenders())
				{
					if (appender is log4net.Appender.FileAppender)
					{
						log4net.Appender.FileAppender fileAppender = (log4net.Appender.FileAppender)appender;
						if (!Path.IsPathRooted(fileAppender.File))
						{
							fileAppender.File = Path.Combine(logFolder, Path.GetFileName(fileAppender.File));
							fileAppender.ActivateOptions();
						}

					}
				}
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
					_octaneManager = new OctaneManager(Octane.Tools.PluginRunMode.ServerPlugin);
					_octaneManager.Init();

				}
				catch (Exception ex)
				{
					var msg = $"Error initializing octane plugin! {ex.Message}";
					TeamFoundationApplicationCore.Log(msg, 1, EventLogEntryType.Error);
					Log.Error(msg);
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
				Log.Info($"ProcessEvent \"{notificationEventArgs.GetType().ToString()}\" for build {updatedEvent.BuildId}");

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
					Log.Info($"ProcessEvent cancelled as Octane is not configured.");
				}
			}
			catch (Exception e)
			{

				Log.Error($"An error \"{e.Message}\" occured during processing of notification.", e);

				TeamFoundationApplicationCore.Log(requestContext, "HPE  : Process Server Event",
					$"The error occured during processing notification: {e}", 123, EventLogEntryType.Error);
			}
			return EventNotificationStatus.ActionPermitted;
		}

		public void Dispose()
		{
			Octane.RestServer.Server.GetInstance().Stop();
		}
	}
}
