using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1.Subscriptions
{
    public class SubscriptionsListResponse
    {
        public int Count { get; set; }
        public List<Object> Value { get; set; }
    }
}
