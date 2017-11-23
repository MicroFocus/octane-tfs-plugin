using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Events
{
    public class CiEventType : IDtoBase
    {
        private string _value;
        private CiEventType(string type)
        {
            _value = type;
        }

        public static CiEventType Undefined => new CiEventType("undefined");
        public static CiEventType Queued => new CiEventType("queued");
        public static CiEventType Started => new CiEventType("started");
        public static CiEventType Finished => new CiEventType("finished");
        public static CiEventType Scm => new CiEventType("scm");

        public override string ToString()
        {
            return _value;
        }
    }
}
