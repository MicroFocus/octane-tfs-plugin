using System;
using MicroFocus.Ci.Tfs.Octane;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans;
using MicroFocus.Ci.Tfs.Octane.Dto.TestResults;

namespace MicroFocus.Ci.Tfs.Tests
{
    [TestClass]
    public class TfsManagerTest : OctaneManagerBaseTest
    {            
        [TestMethod]
        public void GetJobsList()
        {
            _tfsManager.GetJobsList();
        }

        [TestMethod]
        public void GetTestResultsTest()
        {
            TfsBuild build = _tfsManager.GetBuild("DefaultCollection", "Test2", 11);
            TfsTestResults testResults = _tfsManager.GetTestResultsByBuildUri("DefaultCollection", "Test2", build.Uri);
            OctaneTestResult octaneTestResult = TestResultUtils.ConvertToOctaneTestResult("asd", testResults);
            String xml = TestResultUtils.SerializeToXml(octaneTestResult);
        }
    }
}
