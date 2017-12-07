using System.Collections.Generic;
using MicroFocus.Ci.Tfs.Octane.Dto.General;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Events
{
    public class CiEventsList
    {
        public CiServerInfo Server { get; set; }

        public List<CiEvent> Events { get; set; }

        public CiEventsList() => Events = new List<CiEvent>();
    }
}
