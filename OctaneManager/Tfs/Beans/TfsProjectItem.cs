using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans
{
    public class TfsProjectItem : TfsItemBase
    {
        public TfsProjectItem(Guid id, string name) : base(id.ToString(),name)
        {
            
        }
    }
}
