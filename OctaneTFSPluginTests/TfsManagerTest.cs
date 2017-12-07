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
		public void Test()
		{
			var changes = _tfsManager.GetBuildChanges("DefaultCollection", "3086f4e9-d2ef-4f1a-9e48-19bf30c794a5", "48");
			int t = 5;
		}
	}
}
