using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroFocus.Ci.Tfs.Octane.Dto.TestResults
{
    [XmlRoot("test_result")]
    public class OctaneTestResult
    {
        // <build server_id="59fd04f6-041d-43a0-984a-ab27e3edf3bb" job_id="mavenTest" job_name="mavenTest" build_id="6" build_name="6" />
        [XmlElement("build", Order = 1)]
        public OctaneTestResultBuild Build { get; set; }

        //<test_fields>
        //   <test_field type = "Framework" value="TestNG" />
        //  </test_fields>
        [XmlArray("test_fields", Order = 2)]
        [XmlArrayItem("test_field")]
        public List<OctaneTestResultTestField> TestFields;

        //<test_runs>
        //<test_run module = "ui-tests" package="com.hp.devops.demoapp.tests.ui" class="TestA" name="testUIcaseA" duration="0" status="Skipped" started="1511441542659" />

        [XmlArray("test_runs", Order = 3)]
        [XmlArrayItem("test_run")]
        public List<OctaneTestResultTestRun> TestRuns;
    }
}
