using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Dto.Events;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.Events
{
    public class TfsBuildEvent
    {

        public Guid Id { get; set; }
        public string EventType { get; set; }

        public BuildEventResource Resource { get; set; }
        public TfsBuildDefenitionItem Defenition { get; set; }
        public class BuildEventResource
        {
            public Uri Uri { get; set; }
            public int Id { get; set; }
            public string BuildNumber { get; set; }
            public Uri Url { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }

        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public CiEvent ToCiEvent()
        {
            CiEvent ciEvent = new CiEvent();
            switch (EventType)
            {
                case "build.complete":
                    ciEvent.EventType = CiEventType.Finished;
                    break;
                default:
                    ciEvent.EventType = CiEventType.Undefined;
                    break;
            }


            ciEvent.BuildCiId = Resource.Id.ToString();
            ciEvent.Project = GetId(Defenition.Url.ToString());
            ciEvent.Number = Resource.Id.ToString();
            var cause = new CiEventCause();
            switch (Resource.Reason)
            {
                case "manual":
                    cause.CauseType = CiEventCauseType.User;
                    break;
                default:
                    cause.CauseType = CiEventCauseType.Undefined;
                    break;

            }
            ciEvent.Causes.Add(cause);
            ciEvent.StartTime = Resource.StartTime.Ticks;
            ciEvent.Duration = (Resource.EndTime - Resource.StartTime).Ticks;
            ciEvent.ProjectDisplayName = Defenition.Name;
            ciEvent.PhaseType = "post";
            
            

            return ciEvent;
            //            
        }

        private string GetId(string value)
        {
            var elements = value.Split('/');

            var i = value.IndexOf("_apis");
            var id = $"{elements[i - 2]}/{elements[i - 1]}/{Defenition.Name}";
            return id;
        }
    }
}
