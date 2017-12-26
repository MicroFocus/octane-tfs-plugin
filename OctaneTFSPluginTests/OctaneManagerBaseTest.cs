using MicroFocus.Ci.Tfs.Octane;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using MicroFocus.Ci.Tfs.Octane.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SystemConfigurationManager = System.Configuration.ConfigurationManager;

namespace MicroFocus.Ci.Tfs.Tests
{
	[TestClass]
	public class OctaneManagerBaseTest
	{
		protected static OctaneManager octaneManager;
		protected static TfsManager _tfsManager;
		[AssemblyInitialize]
		public static void InitializeConfigs(TestContext context)
		{
			var webbAppUrl = SystemConfigurationManager.AppSettings["webAppUrl"];
			var clientId = SystemConfigurationManager.AppSettings["clientId"];
			var clientSecret = SystemConfigurationManager.AppSettings["clientSecret"];
			var devTimeout = (int)TimeSpan.FromSeconds(int.Parse(SystemConfigurationManager.AppSettings["devTimeout"])).TotalMilliseconds;
			var instanceId = Guid.Parse(SystemConfigurationManager.AppSettings["InstanceId"]);
			var pat = SystemConfigurationManager.AppSettings["pat"];
			var tfsLocation = "http://localhost:8080/tfs";

			var connectionDetails = new ConnectionDetails(webbAppUrl, clientId, clientSecret, tfsLocation, instanceId) { Pat = pat };
			ConfigurationManager.WriteConfig(connectionDetails);


			octaneManager = new OctaneManager(Octane.Tools.PluginRunMode.ConsoleApp, devTimeout);
			_tfsManager = new TfsManager(Octane.Tools.PluginRunMode.ConsoleApp, new Uri(tfsLocation), connectionDetails.Pat);
		}
	}
}
