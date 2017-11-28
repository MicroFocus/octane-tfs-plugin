using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroFocus.Ci.Tfs.Octane.Dto.TestResults
{
    //<test_run module = "ui-tests" package="com.hp.devops.demoapp.tests.ui" class="TestA" name="testUIcaseA" duration="0" status="Skipped" started="1511441542659" />
    public class OctaneTestResultTestRun
    {
        [XmlAttribute("module")]
        public String Module { get; set; }

        [XmlAttribute("package")]
        public String Package { get; set; }

        [XmlAttribute("class")]
        public String Class { get; set; }

        [XmlAttribute("name")]
        public String Name { get; set; }

        [XmlAttribute("duration")]
        public long Duration { get; set; }

        [XmlAttribute("status")]
        public String Status { get; set; }

        [XmlAttribute("started")]
        public long Started { get; set; }

        [XmlElement("error")]
        public OctaneTestResultError Error { get; set; }

    }
}
