﻿using log4net;
using log4net.Config;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using MicroFocus.Ci.Tfs.Octane;
using System;
using System.Reflection;
using System.Threading;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;

[assembly: XmlConfigurator(ConfigFile = @"AlmOctaneTfsPluginConsoleRunner.log4net.config.xml", Watch = true)]
namespace TfsConsolePluginRunner
{
	class Program
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
			Log.Info("");
			Log.Info("");
			Log.Info("******************************************************************");
			Log.Info("***************OctaneTfsConsolePlugin started********************");
			Log.Info("******************************************************************");

            if (!ConfigurationManager.ConfigurationExists())
            {
                ConfigurationManager.WriteConfig(ConfigFileGenerator.GenerateConfig());
            }

            RunModeManager.GetInstance().RunMode = PluginRunMode.ConsoleApp;
			PluginManager pluginManager = PluginManager.GetInstance();
			pluginManager.StartPlugin();
			Console.WriteLine("TFS plugin is running , press any key to exit...");
            Console.ReadLine();

			pluginManager.Shutdown();
			Console.WriteLine("TFS plugin is stopped. The window will close shortly.");
			Thread.Sleep(5000);
		}
    }
}
