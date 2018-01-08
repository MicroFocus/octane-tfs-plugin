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
