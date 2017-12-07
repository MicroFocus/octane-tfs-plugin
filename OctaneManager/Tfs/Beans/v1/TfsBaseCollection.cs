using Newtonsoft.Json;
using System.Collections.Generic;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1
{
    public class TfsBaseCollection <T>
    {
        public int Count { get; set; }

        [JsonProperty("value")]
        public List<T> Items { get; set; }
    }
}
