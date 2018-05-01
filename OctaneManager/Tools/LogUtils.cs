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
using log4net.Config;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity
{
	public class LogUtils
	{
		public static string SPECIAL_LOGGERS_PREFIX = "_";
		public static string TFS_REST_CALLS_LOGGER = SPECIAL_LOGGERS_PREFIX + "TfsRestCalls";
		public static string OCTANE_TEST_RESULTS_LOGGER = SPECIAL_LOGGERS_PREFIX + "MqmTestResults";
		public static string TFS_TEST_RESULTS_LOGGER = SPECIAL_LOGGERS_PREFIX + "TfsTestResults";

		public static void WriteWindowsEvent(string msg, EventLogEntryType eventLogEntryType)
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

		public static string GetLogFilePath(string logType)
		{
			log4net.Repository.ILoggerRepository repository = LogManager.GetRepository();
			foreach (log4net.Appender.IAppender appender in repository.GetAppenders())
			{
				if (appender is log4net.Appender.FileAppender && appender.Name.ToLowerInvariant().StartsWith(logType.ToLower()))
				{
					log4net.Appender.FileAppender fileAppender = (log4net.Appender.FileAppender)appender;
					return fileAppender.File;
				}
			}
			return null;
		}

		public static Dictionary<string, string> GetAllLogFilePaths()
		{
			Dictionary<string, string> logType2LogFilePath = new Dictionary<string, string>();
			log4net.Repository.ILoggerRepository repository = LogManager.GetRepository();
			foreach (log4net.Appender.IAppender appender in repository.GetAppenders())
			{
				if (appender is log4net.Appender.FileAppender)
				{
					log4net.Appender.FileAppender fileAppender = (log4net.Appender.FileAppender)appender;

					string logType = fileAppender.Name.ToLower().Replace("appender", "");
					logType2LogFilePath[logType] = fileAppender.File;
				}
			}
			return logType2LogFilePath;
		}

		public static void ConfigureLog4NetForPluginMode(bool shouldSleep=true)
		{
            if(shouldSleep)
			    Thread.Sleep(10000);
			try
			{
				//find config file to load
				var logConfigFileName = "MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin.log4net.config.xml";

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

					//change path to log files

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
								Paths.CreateDirIfMissing(Paths.LogFolder);
								fileAppender.File = Path.Combine(Paths.LogFolder, Path.GetFileName(fileAppender.File));
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
