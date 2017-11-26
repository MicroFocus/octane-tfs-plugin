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

        public string Outcome { get; set; }

        public DateTime StartedDate { get; set; }

        public DateTime CompletedDate { get; set; }

        public double DurationInMs { get; set; }

        public string ErrorMessage { get; set; }

        public string TestCaseTitle { get; set; }

        public string StackTrace { get; set; }

        public string AutomatedTestName { get; set; }

        public string AutomatedTestType { get; set; }





    }
}
