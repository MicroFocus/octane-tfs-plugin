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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Queue;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SystemConfigurationManager = System.Configuration.ConfigurationManager;

namespace MicroFocus.Ci.Tfs.Tests
{
	[TestClass]
	public class OctaneManagerBaseTest
	{
		protected static QueuesManager octaneManager;
		protected static TfsApis _tfsManager;

		[AssemblyInitialize]
		public static void InitializeConfigs(TestContext context)
		{
			var webbAppUrl = SystemConfigurationManager.AppSettings["webAppUrl"];
			var clientId = SystemConfigurationManager.AppSettings["clientId"];
			var clientSecret = SystemConfigurationManager.AppSettings["clientSecret"];
			var devTimeout = (int)TimeSpan.FromSeconds(int.Parse(SystemConfigurationManager.AppSettings["devTimeout"])).TotalMilliseconds;
			var instanceId = SystemConfigurationManager.AppSettings["InstanceId"];
			var pat = SystemConfigurationManager.AppSettings["pat"];
			var tfsLocation = "http://localhost:8080/tfs";

			var connectionDetails = new ConnectionDetails(webbAppUrl, clientId, clientSecret, tfsLocation, instanceId) { Pat = pat };
			ConfigurationManager.WriteConfig(connectionDetails);

			RunModeManager.GetInstance().RunMode = PluginRunMode.ConsoleApp;
			//octaneManager = new OctaneManager(ConfigurationManager.Read());
			_tfsManager = new TfsApis(tfsLocation, connectionDetails.Pat);
		}
	}
}
