using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems
{
    public class TfsRun
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long TotalTests { get; set; }
        public long PassedTests { get; set; }
        public long UnanalyzedTests { get; set; }
        public string WebAccessUrl { get; set; }
        
    }
}
