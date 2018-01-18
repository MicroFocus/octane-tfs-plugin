/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace MicroFocus.Ci.Tfs.Tests
{
	[TestClass]
	public class OctaneManagerTests : OctaneManagerBaseTest
	{
		[TestMethod]
		public void ConnectionTest()
		{
			octaneManager.Start();

			Thread.Sleep(2000);

			octaneManager.ShutDown();
		}

		[TestMethod]
		public void GetStatusTest()
		{
			octaneManager.Start();
		}

		[TestMethod]
		public void SendResultsTest()
		{
			/*var a= octaneManager.GetScmData("DefaultCollection", "3086f4e9-d2ef-4f1a-9e48-19bf30c794a5", "48");
			var v = 5;
			octaneManager.Init();
			octaneManager.SendTestResults("DefaultCollection", "3086f4e9-d2ef-4f1a-9e48-19bf30c794a5", "50", "DefaultCollection.3086f4e9-d2ef-4f1a-9e48-19bf30c794a5.1", "20171207.2");
			octaneManager.WaitShutdown();*/


		}
	}
}
