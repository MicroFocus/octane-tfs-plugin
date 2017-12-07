using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroFocus.Ci.Tfs.Tests
{
	[TestClass]
	public class OctaneManagerTests : OctaneManagerBaseTest
	{
		[TestMethod]
		public void ConnectionTest()
		{
			octaneManager.Init();

			Thread.Sleep(2000);

			octaneManager.ShutDown();
		}

		[TestMethod]
		public void GetStatusTest()
		{
			octaneManager.Init();



			octaneManager.WaitShutdown();


		}

		[TestMethod]
		public void SendResultsTest()
		{
			octaneManager.Init();
			octaneManager.SendTestResults("DefaultCollection", "3086f4e9-d2ef-4f1a-9e48-19bf30c794a5", "50", "DefaultCollection.3086f4e9-d2ef-4f1a-9e48-19bf30c794a5.1", "20171207.2");
			octaneManager.WaitShutdown();


		}
	}
}
