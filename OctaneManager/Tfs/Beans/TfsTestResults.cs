using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans
{
    public class TfsTestResults
    {
        public int Count { get; set; }

        [JsonProperty("value")]
        public List<TfsTestResult> Results { get; set; }
    }
}
