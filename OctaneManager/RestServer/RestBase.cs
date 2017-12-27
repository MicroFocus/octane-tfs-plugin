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

			Get["/logs/last"] = _ =>
			{
				return HandleGetLastLogRequest();
			};
		}

		private dynamic HandleGetLastLogRequest()
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
