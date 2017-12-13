using Microsoft.VisualStudio.TestTools.UnitTesting;

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
