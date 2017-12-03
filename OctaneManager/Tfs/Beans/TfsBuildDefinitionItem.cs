using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans
{
    public class TfsBuildDefinitionItem : TfsItemBase
    {
        public Uri Url { get; set; }
        public TfsBuildDefinitionItem(string id, string name) : base(id, name)
        {
            
        }        
    }
}
