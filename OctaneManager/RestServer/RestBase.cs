using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.Events;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;

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
                Log.Debug($"Recieved build event : {text}");


                HandleBuildEvent(text);
                return "Recieved";
            };
        }

        private void HandleBuildEvent(string body)
        {
            try
            {
                var buildEvent = JsonConvert.DeserializeObject<TfsBuildEvent>(body);

                //CiEvent ciEvent = buildEvent.Resource

                BuildEvent?.Invoke(this, new CiEvent());
            }
            catch (Exception ex)
            {
                Log.Error("Error parsing build event body");
            }
        }
    }
}
