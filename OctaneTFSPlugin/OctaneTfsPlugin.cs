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

		
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static OctaneManagerInitializer _octaneManagerInitializer = new OctaneManagerInitializer();

		static OctaneTfsPlugin()
		{

			ConfigureLog4Net();
			_octaneManagerInitializer = new OctaneManagerInitializer();
			_octaneManagerInitializer.Start(Octane.Tools.PluginRunMode.ServerPlugin);
			Log.Info("OctaneTfsPlugin started");
		}

		private static void ConfigureLog4Net()
		{
			try
			{
				//find config file to load
				var logConfigFileName = "OctaneTFSPluginLogConfig.xml";

				var dllPath = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath;
				var pluginsDirectory = Path.GetDirectoryName(dllPath);

				string fullPath = null;
				if (File.Exists(logConfigFileName)) //check on C://windows/system32
				{
					fullPath = logConfigFileName;
				}
				else
				{
					string temp = Path.Combine(pluginsDirectory, logConfigFileName);
					if (File.Exists(temp))
					{
						fullPath = temp;
					}
				}
				if (fullPath != null)
				{
					
					XmlConfigurator.ConfigureAndWatch(new FileInfo(fullPath));
					TeamFoundationApplicationCore.Log($"Log4net configuration file {fullPath}", 124, EventLogEntryType.Information);

					//change path to log file for unrooted files (like 'abc.log')
					var logFolder = Path.Combine(ConfigurationManager.ConfigFolder, "Logs");
					log4net.Repository.ILoggerRepository repository = LogManager.GetRepository();
					foreach (log4net.Appender.IAppender appender in repository.GetAppenders())
					{
						if (appender is log4net.Appender.FileAppender)
						{
							log4net.Appender.FileAppender fileAppender = (log4net.Appender.FileAppender)appender;
							if (fileAppender.File.Contains("TfsJobAgent"))
							//it means - config file contains only name of log file, for example abc.log. Directory path was added automatically.
							//usually app doesn't have permissions to write logs in plugin directory
							{
								if (!Directory.Exists(logFolder))
								{
									Directory.CreateDirectory(logFolder);
								}

								fileAppender.File = Path.Combine(logFolder, Path.GetFileName(fileAppender.File));
								fileAppender.ActivateOptions();
							}
							TeamFoundationApplicationCore.Log($"Log4net log file is  {fileAppender.File}", 125, EventLogEntryType.Information);
						}
					}
				}
			}
			catch (Exception e)
			{
				TeamFoundationApplicationCore.LogException("Failed to initialize log4net : " + e.Message, e);
			}
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
