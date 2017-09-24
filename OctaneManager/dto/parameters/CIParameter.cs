using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.dto.parameters
{
    class CIParameter
    {
        public string ParameterType { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public string DefaultValue { get; set; }
    }
}
