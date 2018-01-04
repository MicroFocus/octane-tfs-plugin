using log4net;
using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.Events;
using MicroFocus.Ci.Tfs.Octane.Tools;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using System;
using System.IO;
using System.Reflection;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using System.Text;

namespace MicroFocus.Ci.Tfs.Octane.RestServer
{
	public class RestBase : NancyModule
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static event EventHandler<CiEvent> BuildEvent;

		public RestBase()
		{
			Get["/"] = _ =>
			{
				if (OctaneManagerInitializer.GetInstance().IsOctaneInitialized())
				{
					return "OctaneManager is initialized";
				}
				return "OctaneManager still is not initialized";
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

			//TODO add defence - sensitive action
			Post["/config"] = _ =>
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
					return new TextResponse(msg).WithStatusCode(400);
				}

				return "Configuration changed";
			};

			Post["/config/test"] = _ =>
			{
				try
				{
					var configStr = Context.Request.Body.AsString();
					var config = JsonHelper.DeserializeObject<ConnectionDetails>(configStr);

					ConnectionCreator.CheckMissingValues(config);
					ConnectionCreator.CreateTfsConnection(PluginRunMode.ServerPlugin, config);
					ConnectionCreator.CreateOctaneConnection(config);
				}
				catch (Exception e)
				{
					return new TextResponse(e.Message).WithStatusCode(400);
				}

				return "";
			};

			Get["/config"] = _ =>
			{
				var assembly = Assembly.GetExecutingAssembly();
				var resourceName = "MicroFocus.Ci.Tfs.Octane.RestServer.Views.config.html";
				string result;
				using (Stream stream = assembly.GetManifestResourceStream(resourceName))
				using (StreamReader reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}

				result = result.Replace("//{defaultConf}", "var defaultConf =" + JsonHelper.SerializeObject(ConfigurationManager.Read()));

				return result;
				//string config = JsonHelper.SerializeObject(ConfigurationManager.Read().RemoveSensitiveInfo(), true);
				//return new TextResponse(config);
			};


			//TODO add defence - sensitive action
			Post["/start"] = _ =>
			{
				if (OctaneManagerInitializer.GetInstance().IsOctaneInitialized())
					return "Octane manager is already running";

				Log.Debug("plugin start requested");

				OctaneManagerInitializer.GetInstance().StartPlugin();
				return "Start plugin requested";
			};

			//TODO add defence - sensitive action
			Post["/stop"] = _ =>
			{
				Log.Debug("plugin stop requested");
				OctaneManagerInitializer.GetInstance().StopPlugin();
				return "Stop plugin requested";
			};


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
				return new TextResponse($"Log file with index {logId} is not exist").WithStatusCode(404); ;
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
