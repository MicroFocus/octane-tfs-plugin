using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans
{
    public class TfsTestResult
    {
        public long Id { get; set; }

        public String Outcome { get; set; }

        public DateTime StartedDate { get; set; }

        public DateTime CompletedDate { get; set; }

        public double DurationInMs { get; set; }

        public String ErrorMessage { get; set; }

        public String TestCaseTitle { get; set; }

        public String StackTrace { get; set; }

        public String AutomatedTestName { get; set; }

        
            
            
            
    }
}
