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
			TfsBuild build = _tfsManager.GetBuild("DefaultCollection", "3086f4e9-d2ef-4f1a-9e48-19bf30c794a5", "20171204.9");
			TfsTestResults testResults = _tfsManager.GetTestResultsByBuildUri("DefaultCollection", "Test2", build.Uri);
			OctaneTestResult octaneTestResult = TestResultUtils.ConvertToOctaneTestResult("77393d1d-c40c-439d-9fda-0b88b913a95f", "DefaultCollection.3086f4e9-d2ef-4f1a-9e48-19bf30c794a5.1", "20171203.13", testResults);
			String xml = TestResultUtils.SerializeToXml(octaneTestResult);
		}
	}
}
