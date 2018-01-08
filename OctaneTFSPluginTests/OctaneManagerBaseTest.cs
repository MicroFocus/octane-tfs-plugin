﻿using MicroFocus.Ci.Tfs.Octane;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
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
			var instanceId = SystemConfigurationManager.AppSettings["InstanceId"];
			var pat = SystemConfigurationManager.AppSettings["pat"];
			var tfsLocation = "http://localhost:8080/tfs";

			var connectionDetails = new ConnectionDetails(webbAppUrl, clientId, clientSecret, tfsLocation, instanceId) { Pat = pat };
			ConfigurationManager.WriteConfig(connectionDetails);

			RunModeManager.GetInstance().RunMode = PluginRunMode.ConsoleApp;
			octaneManager = new OctaneManager(ConfigurationManager.Read(), devTimeout);
			_tfsManager = new TfsManager(new Uri(tfsLocation), connectionDetails.Pat);
		}
	}
}
