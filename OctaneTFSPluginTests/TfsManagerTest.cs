using System;
using MicroFocus.Ci.Tfs.Octane;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans;
using MicroFocus.Ci.Tfs.Octane.Dto.TestResults;
using MicroFocus.Ci.Tfs.Octane.Dto.Scm;
using System.Collections.Generic;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1.SCM;

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


		}
	}
}
