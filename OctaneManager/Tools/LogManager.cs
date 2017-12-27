using log4net;
using log4net.Config;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace MicroFocus.Ci.Tfs.Octane.Tools
{
	public class LogUtils
	{
		private static void WriteWindowsEvent(string msg, EventLogEntryType eventLogEntryType)
		{
			string source = "TFSJobAgent";
			string log = "Application";

			if (!EventLog.SourceExists(source))
			{
				EventLog.CreateEventSource(source, log);

				//An event log source should not be created and immediately used.
				//There is a latency time to enable the source, it should be created
				//prior to executing the application that uses the source.
				Thread.Sleep(1000);
				if (!EventLog.SourceExists(source))
				{
					//ignore writing if source is not created 
					return;
				}
			}

			EventLog.WriteEntry(source, msg, eventLogEntryType, 5005);
		}

		public static void ConfigureLog4NetForPluginMode()
		{
			Thread.Sleep(10000);
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
					WriteWindowsEvent($"Log4net configuration file {fullPath}", EventLogEntryType.Information);

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
							WriteWindowsEvent($"Log4net log file is  {fileAppender.File}", EventLogEntryType.Information);
						}
					}
				}
			}
			catch (Exception e)
			{
				WriteWindowsEvent($"Failed to initialize log4net : e.Message.{Environment.NewLine}{e.StackTrace}", EventLogEntryType.Error);
			}
		}
	}
}
