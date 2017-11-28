using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans
{
    public class TfsBuildDefenitionItem : TfsItemBase
    {
        public Uri Url { get; set; }
        public TfsBuildDefenitionItem(string id, string name) : base(id, name)
        {
            
        }        
    }
}
