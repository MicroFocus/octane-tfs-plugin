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

            octaneManager.SendTestResults("Aa","bb");

            octaneManager.WaitShutdown();


        }
    }
}
