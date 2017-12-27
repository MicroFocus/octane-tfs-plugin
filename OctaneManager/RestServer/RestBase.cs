using log4net;
using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.Events;
using MicroFocus.Ci.Tfs.Octane.Tools;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
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
				var text = Context.Request.Body.AsString();
				Log.Debug($"Received build event : {text}");
				HandleBuildEvent(text);
				return "Received";
			};

			Get["/logs/last"] = _ =>
			{
				DynamicDictionaryValue format = ((DynamicDictionary)Request.Query)["format"];
				return HandleGetLastLogRequestWIthString(format.HasValue ? format.ToString() : null);
			};
		}

		private dynamic HandleGetLastLogRequestWIthString(string format)
		{
			string path = LogUtils.GetLogFilePath();
			if (path == null)
			{
				return "Log file is not defined";
			}
			if (!File.Exists(path))
			{
				return $"Log file <{path}> is not exist";
			}

			var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			string contents;
			using (var sr = new StreamReader(fileStream))
			{
				contents = sr.ReadToEnd();
			}
			return new TextResponse(contents);
		}


		private dynamic HandleGetLastLogRequest1(string format)
		{
			string path = LogUtils.GetLogFilePath();
			if (path == null)
			{
				return "Log file is not defined";
			}
			if (!File.Exists(path))
			{
				return $"Log file <{path}> is not exist";
			}

			string gzipFilePath = CompressToFile(path);
			var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			var response = new StreamResponse(() => file, "text/plain");

			//return response.WithHeader("Content-Encoding", "gzip");
			return response;
		}

		public static string CompressToFile(string path)
		{
			string gzippedPath = path + ".gz";
			// Get the stream of the source file.
			using (FileStream inFile = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				// Create the compressed file.
				using (FileStream outFile = File.Create(gzippedPath))
				{
					using (GZipStream Compress = new GZipStream(outFile, CompressionMode.Compress))
					{
						inFile.CopyTo(Compress);
					}
				}
			}
			return gzippedPath;
		}

		private void HandleBuildEvent(string body)
		{
			try
			{
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
