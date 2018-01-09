using log4net;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using System;
using System.IO;
using System.Reflection;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using System.Text;
using MicroFocus.Ci.Tfs.Octane;


namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.RestServer
{
	public class RestBase : NancyModule
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static event EventHandler<CiEvent> BuildEvent;

		public RestBase()
		{
			Get["/"] = _ =>
			{
				if (PluginManager.GetInstance().IsInitialized())
				{
					return "Plugin is active";
				}
				return "Plugin is not active";
			};

			Post["/build-event/"] = _ =>
			{
				HandleBuildEvent();
				return "Received";
			};

			Get["/logs/{logId}"] = parameters =>
			{
				return HandleGetLogRequest(parameters.logId);
			};

			Get["/logs"] = _ =>
			{
				return HandleGetLogListRequest();
			};

			Post["/config", RestrictAccessFromLocalhost] = _ =>
			{
				var configStr = Context.Request.Body.AsString();
				Log.Debug($"Received new config: \n {configStr}");

				var config = JsonHelper.DeserializeObject<ConnectionDetails>(configStr);

				try
				{
					ConnectionCreator.CheckMissingValues(config);
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

			Post["/config/test", RestrictAccessFromLocalhost] = _ =>
			{
				try
				{
					var configStr = Context.Request.Body.AsString();
					var config = JsonHelper.DeserializeObject<ConnectionDetails>(configStr);

					ConnectionCreator.CheckMissingValues(config);
					ConnectionCreator.CreateTfsConnection(config);
					ConnectionCreator.CreateOctaneConnection(config);
				}
				catch (Exception e)
				{
					return new TextResponse(e.Message).WithStatusCode(HttpStatusCode.BadRequest);
				}

				return "";
			};

			Get["/config", RestrictAccessFromLocalhost] = _ =>
			{
				var assembly = Assembly.GetExecutingAssembly();
				var resourceName = "MicroFocus.Ci.Tfs.Octane.RestServer.Views.config.html";
				string result;
				using (Stream stream = assembly.GetManifestResourceStream(resourceName))
				using (StreamReader reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}

				ConnectionDetails conf = null;
				try
				{
					conf = ConfigurationManager.Read();
				}
				catch (FileNotFoundException)
				{
					conf = new ConnectionDetails();
					conf.TfsLocation = ConnectionCreator.GetTfsLocationFromHostName();
				}

				string confJson = JsonHelper.SerializeObject(conf);
				result = result.Replace("//{defaultConf}", "var defaultConf =" + confJson);

				return result;
				//string config = JsonHelper.SerializeObject(ConfigurationManager.Read().RemoveSensitiveInfo(), true);
				//return new TextResponse(config);
			};

			Get["/resources/{resourceName}"] = parameters =>
			{
				var assembly = Assembly.GetExecutingAssembly();
				var resourceName = $"MicroFocus.Ci.Tfs.Octane.RestServer.Views.Resources.{parameters.resourceName}";
				Stream stream = assembly.GetManifestResourceStream(resourceName);
				if (stream == null)
				{
					return new TextResponse("Resource not found").WithStatusCode(HttpStatusCode.NotFound);
				}

				var response = new StreamResponse(() => stream, MimeTypes.GetMimeType(resourceName));
				return response;
			};

			Post["/start", RestrictAccessFromLocalhost] = _ =>
			{
				if (PluginManager.GetInstance().IsInitialized())
					return "Octane manager is already running";

				Log.Debug("Plugin start requested");

				PluginManager.GetInstance().StartPlugin();
				return "Started";
			};

			Post["/stop", RestrictAccessFromLocalhost] = _ =>
			{
				Log.Debug("Plugin stop requested");
				PluginManager.GetInstance().StopPlugin();
				return "Stopped";
			};

			Post["/clear-event-queues", RestrictAccessFromLocalhost] = _ =>
			{
				Log.Debug("Clear event queues requested");
				PluginManager.GetInstance().EventManager.ClearQueues();
				return "Cleared";
			};

			Get["/event-queue-status"] = _ =>
			{
				if (PluginManager.GetInstance().IsInitialized())
				{
					var queueStatus = PluginManager.GetInstance().EventManager.GetQueueStatus();
					string json = JsonHelper.SerializeObject(queueStatus, true);
					return json;
				}
				else
				{
					return "Plugin is not active";
				}
			};

		}

		private bool RestrictAccessFromLocalhost(NancyContext ctx)
		{

			string host = ctx.Request.Url.HostName.ToLower();
			if (!("localhost".Equals(host) || "127.0.0.1".Equals(host)))
			{
				return false;
			}
			return true;
		}

		private dynamic HandleGetLogListRequest()
		{
			string logfilePath = LogUtils.GetLogFilePath();
			if (logfilePath == null)
			{
				return "Log file is not defined";
			}
			string logDirPath = Path.GetDirectoryName(logfilePath);
			string logFileName = Path.GetFileName(logfilePath);
			string[] foundFiles = Directory.GetFiles(logDirPath, logFileName + ".?");
			StringBuilder sb = new StringBuilder();
			sb.Append($"Found {foundFiles.Length} files");
			foreach (string foundFile in foundFiles)
			{
				sb.Append("</br>").Append(Environment.NewLine);
				string fileName = Path.GetFileName(foundFile);
				if (logFileName.Equals(fileName))
				{
					sb.Append("/logs/last");
				}
				else
				{
					sb.Append("/logs/" + fileName.Replace(logFileName + ".", ""));
				}
			}
			return sb.ToString();
		}

		private dynamic HandleGetLogRequest(string logId)
		{
			string path = LogUtils.GetLogFilePath();
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
				var ciEvent = buildEvent.ToCiEvent();
				BuildEvent?.Invoke(this, ciEvent);
			}
			catch (Exception ex)
			{
				Log.Error($"Error parsing build event body", ex);
			}
		}
	}
}
