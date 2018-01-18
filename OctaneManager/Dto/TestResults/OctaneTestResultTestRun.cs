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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.TestResults
{
    //<test_run module = "ui-tests" package="com.hp.devops.demoapp.tests.ui" class="TestA" name="testUIcaseA" duration="0" status="Skipped" started="1511441542659" />
    public class OctaneTestResultTestRun
    {
        [XmlAttribute("module")]
        public string Module { get; set; }

        [XmlAttribute("package")]
        public string Package { get; set; }

        [XmlAttribute("class")]
        public string Class { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("duration")]
        public long Duration { get; set; }

        [XmlAttribute("status")]
        public string Status { get; set; }

        [XmlAttribute("started")]
        public long Started { get; set; }

        [XmlElement("error")]
        public OctaneTestResultError Error { get; set; }

        [XmlAttribute("external_report_url")]
        public string ExternalReportUrl { get; set; }

    }
}
