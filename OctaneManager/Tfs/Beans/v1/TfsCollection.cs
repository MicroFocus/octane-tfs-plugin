using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1
{
    public class TfsCollection <T>
    {
        public int Count { get; set; }

        [JsonProperty("value")]
        public List<T> Results { get; set; }
    }
}
