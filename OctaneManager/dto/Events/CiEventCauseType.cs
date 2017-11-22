using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Events
{
    internal class CiEventCauseType : IDtoBase
    {
        private string _value;
        private CiEventCauseType(string type)
        {
            _value = type;
        }

        public static CiEventCauseType Scm => new CiEventCauseType("scm");
        public static CiEventCauseType User => new CiEventCauseType("user");
        public static CiEventCauseType Timer => new CiEventCauseType("timer");
        public static CiEventCauseType Upstream => new CiEventCauseType("upstream");
        public static CiEventCauseType Undefined => new CiEventCauseType("undefined");

        public override string ToString()
        {
            return _value;
        }
    }
}
