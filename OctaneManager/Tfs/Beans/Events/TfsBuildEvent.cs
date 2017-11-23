using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.Dto;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.Events
{
    public class TfsBuildEvent
    {

        public Guid Id { get; set; }        
        public string EventType { get; set; }

        public BuildEventResource Resource { get; set; }

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

            return ciEvent;
//            ciEvent.BuildCiId = Resource.BuildNumber;
//            ciEvent.EventType = 
        }
    }
}
