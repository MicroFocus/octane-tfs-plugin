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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity;
using MicroFocus.Ci.Tfs.Octane;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.RestServer
{
    public class RestBase : NancyModule
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static string PATH_TO_RESOURCE = "MicroFocus.Adm.Octane.CiPlugins.Tfs.Core";

        public RestBase()
        {
            Get["/"] = _ =>
            {
                String view = GetView("console.html");
                return view;
            };

            Get["/status"] = _ =>
            {
                Dictionary<string, object> map = new Dictionary<string, object>();
                map["pluginStatus"] = PluginManager.GetInstance().Status.ToString();
                map["pluginVersion"] = Helpers.GetPluginVersion();
                map["tfsVersion"] = RunModeManager.GetInstance().TfsVersion.ToString();
                map["generalEventsQueue"] = PluginManager.GetInstance().GeneralEventsQueue.Count;
                map["scmEventsQueue"] = PluginManager.GetInstance().ScmEventsQueue.Count;
                map["testResultsQueue"] = PluginManager.GetInstance().TestResultsQueue.Count;

                map["isLocal"] = AllowConfigurationModifyAccess(Request);

                return map;
            };

            Post["/build-event/"] = _ =>
            {
                RunModeManager runModeManager = RunModeManager.GetInstance();
                if (runModeManager.RunMode == PluginRunMode.ConsoleApp)
                {
                    HandleBuildEvent();
                }

                return "Received";
            };

            Get["/logs/{logType}/{logId}"] = parameters =>
            {
                return HandleGetLogRequest(parameters.logType, parameters.logId);
            };

            Get["/logs"] = parameters =>
            {
                String view = GetView("logs.html");
                return view;
                //return HandleGetLogListRequest();
            };

            Get["/logs/download/all"] = _ =>
            {
                string zipPath = CreateZipFileFromLogFiles();
                string fileName = Path.GetFileName(zipPath);
                DeleteTempZipFileWithDelay(zipPath);

                var file = new FileStream(zipPath, FileMode.Open);
                var response = new StreamResponse(() => file, MimeTypes.GetMimeType(fileName));
                return response.AsAttachment(fileName);
            };

            Post["/config", AllowConfigurationModifyAccess] = _ =>
            {
                var configStr = Context.Request.Body.AsString();
                Log.Debug($"Received new configuration");//dont log received log configuration as it contains plain passwords
                var config = JsonHelper.DeserializeObject<ConnectionDetails>(configStr);
                try
                {
                    ConnectionCreator.CheckMissingValues(config);
                    ConfigurationManager.ResetSensitiveInfo(config);
                    ConfigurationManager.WriteConfig(config);
                }
                catch (Exception e)
                {
                    string msg = "Failed to save configuration" + e.Message;
                    Log.Error(msg, e);
                    return new TextResponse(msg).WithStatusCode(HttpStatusCode.BadRequest);
                }

                return "Configuration changed";
            };

            Post["/config/test", AllowConfigurationModifyAccess] = _ =>
            {
                try
                {
                    var configStr = Context.Request.Body.AsString();
                    var config = JsonHelper.DeserializeObject<ConnectionDetails>(configStr);

                    ConnectionCreator.CheckMissingValues(config);
                    ConfigurationManager.ResetSensitiveInfo(config);
                    ConnectionCreator.CreateTfsConnection(config);
                    ConnectionCreator.CreateOctaneConnection(config);
                }
                catch (Exception e)
                {
                    return new TextResponse(e.Message).WithStatusCode(HttpStatusCode.BadRequest);
                }

                return "";
            };

            Get["/config"] = _ =>
            {
                if (AllowConfigurationModifyAccess(Request))
                {
                    String view = GetView("config.html");
                    ConnectionDetails conf = null;
                    try
                    {
                        conf = ConfigurationManager.Read(false).GetInstanceWithoutSensitiveInfo();
                    }
                    catch (FileNotFoundException)
                    {
                        conf = new ConnectionDetails();
                        conf.TfsLocation = ConnectionCreator.GetTfsLocationFromHostName();
                    }

                    string confJson = JsonHelper.SerializeObject(conf);
                    view = view.Replace("//{defaultConf}", "var defaultConf =" + confJson);

                    return view;
                }
                else
                {
                    string prefix = "Configuration is read-only. To modify configuration, access http://localhost:4567/config on the TFS machine.";
                    string config = JsonHelper.SerializeObject(ConfigurationManager.Read(false).GetInstanceWithoutSensitiveInfo(), true);

                    return new TextResponse(prefix + Environment.NewLine + config);
                }
            };

            Get["/resources/{resourceName}"] = parameters =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"{PATH_TO_RESOURCE}.RestServer.Views.Resources.{parameters.resourceName}";
                Stream stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    return new TextResponse("Resource not found").WithStatusCode(HttpStatusCode.NotFound);
                }

                var response = new StreamResponse(() => stream, MimeTypes.GetMimeType(resourceName));
                return response;
            };

            Post["/start", AllowConfigurationModifyAccess] = _ =>
            {
                if (PluginManager.GetInstance().Status == PluginManager.StatusEnum.Connected)
                    return "ALM Octane plugin is already running";

                Log.Debug("Plugin start requested");

                PluginManager.GetInstance().StartPlugin();
                return "Starting ALM Octane plugin";
            };

            Post["/stop", AllowConfigurationModifyAccess] = _ =>
            {
                Log.Debug("Plugin stop requested");
                PluginManager.GetInstance().StopPlugin(false);
                return "Stopping ALM Octane plugin";
            };

            Post["/queues/clear", AllowConfigurationModifyAccess] = _ =>
            {
                Dictionary<string, object> queueStatus = new Dictionary<string, object>();
                queueStatus["GeneralEventsQueue"] = PluginManager.GetInstance().GeneralEventsQueue.Count;
                queueStatus["ScmEventsQueue"] = PluginManager.GetInstance().ScmEventsQueue.Count;
                queueStatus["TaskResultQueue"] = PluginManager.GetInstance().TestResultsQueue.Count;

                string json = JsonHelper.SerializeObject(queueStatus, true);
                Log.Debug($"Clear event queues requested : {json}");

                PluginManager.GetInstance().GeneralEventsQueue.Clear();
                PluginManager.GetInstance().ScmEventsQueue.Clear();
                PluginManager.GetInstance().TestResultsQueue.Clear();
                return $"Cleared {json}";
            };
        }

        private static void DeleteTempZipFileWithDelay(string fileName)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Thread.Sleep(60 * 2 * 1000);//delete after 2 minutes
                    File.Delete(fileName);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to delete zip file {fileName} : {e.Message}");
                }
            });

        }

        private static string CreateZipFileFromLogFiles()
        {
            //get log file names
            string[] patterns = new[] { "*.log", "*.log.1", "*.log.2" };
            List<string> fileList = new List<string>();
            foreach (string pattern in patterns)
            {
                var arr = Directory.GetFiles(Paths.LogFolder, pattern, SearchOption.TopDirectoryOnly);
                fileList.AddRange(arr);
            }

            //create log temp dir
            string dir = Path.Combine(Paths.LogFolder, "temp");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            //create zip file
            string zipFileName = Path.Combine(dir, "Logs " + DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.fffffff") + ".zip");
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
            {
                foreach (var file in fileList)
                {
                    using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
                    {
                        var zipArchiveEntry = zip.CreateEntry(Path.GetFileName(file), CompressionLevel.Optimal);
                        using (var destination1 = zipArchiveEntry.Open())
                        {
                            stream.CopyTo(destination1);
                        }
                    }
                }
            }
            return zipFileName;
        }

        private static string GetView(string viewName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{PATH_TO_RESOURCE}.RestServer.Views.{viewName}";
            string view = "";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                view = reader.ReadToEnd();
            }
            return view;
        }

        private bool AllowConfigurationModifyAccess(NancyContext ctx)
        {
            return AllowConfigurationModifyAccess(ctx.Request);
        }

        private bool AllowConfigurationModifyAccess(Request request)
        {
            if (RunModeManager.GetInstance().RestrictConfigurationAccessFromLocalhost)
            {
                string host = request.Url.HostName.ToLower();
                return "localhost".Equals(host) || "127.0.0.1".Equals(host);
            }
            else
            {
                return true;
            }
        }

        private dynamic HandleGetLogListRequest()
        {
            Dictionary<string, string> logType2LogFilePath = LogUtils.GetAllLogFilePaths();
            StringBuilder sb = new StringBuilder();
            int counter = 0;
            foreach (KeyValuePair<string, string> entry in logType2LogFilePath)
            {
                string logDirPath = Path.GetDirectoryName(entry.Value);
                string logFileName = Path.GetFileName(entry.Value);
                string[] foundFiles = Directory.GetFiles(logDirPath, logFileName + ".?");

                foreach (string foundFile in foundFiles)
                {
                    counter++;
                    sb.Append("</br>").Append(Environment.NewLine);
                    string fileName = Path.GetFileName(foundFile);
                    if (logFileName.Equals(fileName))
                    {
                        sb.Append($"/logs/{entry.Key}/last");
                    }
                    else
                    {
                        sb.Append($"/logs/{entry.Key}/" + fileName.Replace(logFileName + ".", ""));
                    }
                }
            }

            sb.Insert(0, $"Found {counter} files : ");
            return sb.ToString();
        }

        private static dynamic HandleGetLogRequest(string logType, string logId)
        {
            string path = LogUtils.GetLogFilePath(logType);
            if (path == null)
            {
                return "Log file is not defined";
            }

            if (logId != null && (logId.ToLower().Equals("last") || logId.Equals("0")))
            {
                //don't change path
            }
            else
            {
                path = path + "." + logId;
            }
            if (!File.Exists(path))
            {
                return new TextResponse($"Log file with index {logId} is not exist").WithStatusCode(HttpStatusCode.NotFound);
            }

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            string contents;
            using (var sr = new StreamReader(fileStream))
            {
                contents = sr.ReadToEnd();
            }
            return new TextResponse(contents);
        }

        private void HandleBuildEvent()
        {
            try
            {
                var body = Context.Request.Body.AsString();
                Log.Debug($"Received build event : {body}");
                var buildEvent = JsonHelper.DeserializeObject<TfsBuildEvent>(body);
                var finishEvent = buildEvent.ToCiEvent();

                var startEvent = finishEvent.Clone();
                startEvent.EventType = CiEventType.Started;

                PluginManager.GetInstance().GeneralEventsQueue.Add(startEvent);
                PluginManager.GetInstance().HandleFinishEvent(finishEvent);
            }
            catch (Exception ex)
            {
                Log.Error($"Error parsing build event body", ex);
            }
        }
    }
}
