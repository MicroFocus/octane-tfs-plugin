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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools
{
    public class LogUtils
    {
        public static readonly string SPECIAL_LOGGERS_PREFIX = "_";
        public static readonly string TFS_REST_CALLS_LOGGER = SPECIAL_LOGGERS_PREFIX + "TfsRestCalls";
        public static readonly string TFS_TEST_RESULTS_LOGGER = SPECIAL_LOGGERS_PREFIX + "TfsTestResults";
        public static readonly string OCTANE_TEST_RESULTS_LOGGER = SPECIAL_LOGGERS_PREFIX + "OctaneTestResults";
        public static readonly string TASK_POLLING_LOGGER = SPECIAL_LOGGERS_PREFIX + "OctaneTaskPolling";

        public static void WriteWindowsEvent(string msg, EventLogEntryType eventLogEntryType, string source = "TFSJobAgent")
        {
            const string log = "Application";

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
            var repository = LogManager.GetRepository();
            foreach (log4net.Appender.IAppender appender in repository.GetAppenders())
            {
                if (appender is FileAppender && appender.Name.ToLowerInvariant().StartsWith(logType.ToLower()))
                {
                    var fileAppender = (FileAppender)appender;
                    return fileAppender.File;
                }
            }

            //if not found
            String generalPath = $"C: \\Users\\Public\\Documents\\OctaneTfsPlugin\\logs\\{logType}.log";
            if (File.Exists(generalPath))
            {
                return generalPath;
            }

            return null;
        }

        public static Dictionary<string, string> GetAllLogFilePaths()
        {
            var logType2LogFilePath = new Dictionary<string, string>();
            var repository = LogManager.GetRepository();
            foreach (var appender in repository.GetAppenders())
            {
                if (!(appender is FileAppender fileAppender)) continue;

                var logType = fileAppender.Name.ToLower().Replace("appender", "");
                logType2LogFilePath[logType] = fileAppender.File;
            }
            return logType2LogFilePath;
        }

        public static void ConfigureLog4NetForPluginMode()
        {
            try
            {
                //find config file to load
                const string logConfigFileName = "MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin.log4net.config.xml";

                var dllPath = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath;
                var pluginsDirectory = Path.GetDirectoryName(dllPath);

                string fullPath = null;
                if (File.Exists(logConfigFileName)) //check on C://windows/system32
                {
                    fullPath = logConfigFileName;
                }
                else
                {
                    var temp = Path.Combine(pluginsDirectory, logConfigFileName);
                    if (File.Exists(temp))
                    {
                        fullPath = temp;
                    }
                }
                if (fullPath != null)
                {
                    Paths.CreateDirIfMissing(Paths.LogFolder);
                    XmlConfigurator.ConfigureAndWatch(new FileInfo(fullPath));
                    WriteWindowsEvent($"Log4net configuration file {fullPath}", EventLogEntryType.Information);



                    /*
                    //change path to log files
                    var repository = LogManager.GetRepository();
					foreach (log4net.Appender.IAppender appender in repository.GetAppenders())
					{
						if (appender is FileAppender fileAppender)
						{
						    if (fileAppender.File.Contains("TfsJobAgent"))
							//it means - config file contains only name of log file, for example abc.log. Directory path was added automatically.
							//usually app doesn't have permissions to write logs in plugin directory
							{
								
								fileAppender.File = Path.Combine(Paths.LogFolder, Path.GetFileName(fileAppender.File));
								fileAppender.ActivateOptions();
							}
							WriteWindowsEvent($"Log4net log file is  {fileAppender.File}", EventLogEntryType.Information);
						}
					}*/
                }
            }
            catch (Exception e)
            {
                WriteWindowsEvent($"Failed to initialize log4net : e.Message.{Environment.NewLine}{e.StackTrace}", EventLogEntryType.Error);
            }
        }

        public static void ConfigureLog4NetForSetup()
        {
            String path = @"C:\Users\Public\Documents\OctaneTfsPlugin\logs\setup.log";
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = true;
            roller.File = path;
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 2;
            roller.MaximumFileSize = "3MB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();
            roller.LockingModel = new FileAppender.MinimalLock();
            hierarchy.Root.AddAppender(roller);

            MemoryAppender memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = Level.Debug;
            hierarchy.Configured = true;
        }
    }
}
