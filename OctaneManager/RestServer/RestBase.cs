using log4net;
using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.Events;
using Nancy;
using Nancy.Extensions;
using System;
using System.Reflection;

namespace MicroFocus.Ci.Tfs.Octane.RestServer
{
	public class RestBase : NancyModule
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static event EventHandler<CiEvent> BuildEvent;

		public RestBase()
		{
			Get["/"] = _ => "Server is up and working!";

			Post["/build-event/"] = _ =>
			{
				var text = Context.Request.Body.AsString();
				Log.Debug($"Received build event : {text}");
				HandleBuildEvent(text);
				
				return "Received";
			};
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
