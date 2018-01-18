/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.ApiItems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans
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

        public string FailureType { get; set; }

        public string AutomatedTestName { get; set; }

        public string AutomatedTestType { get; set; }

        public string AutomatedTestStorage { get; set; }

        public TfsBaseItem Project { get; set; }

        public TfsBaseItem Build { get; set; }

        public TfsBaseItem TestRun { get; set; }
    }
}
