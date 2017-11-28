using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto;
using MicroFocus.Ci.Tfs.Octane.Dto.Events;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Snapshots
{
    public class CiBuildResult : IDtoBase
    {
        private readonly string _value;
        private CiBuildResult(string type)
        {
            _value = type;
        }

        public static CiBuildResult Unavailable => new CiBuildResult("unavailable");
        public static CiBuildResult Unstable => new CiBuildResult("unstable");
        public static CiBuildResult Aborted => new CiBuildResult("aborted");
        public static CiBuildResult Failure => new CiBuildResult("failure");
        public static CiBuildResult Success => new CiBuildResult("success");

        public override string ToString()
        {
            return _value;
        }
    }
}
