using System;
using System.Reflection;
using log4net;
using log4net.Config;
using MicroFocus.Ci.Tfs.Octane;
using System.Threading;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;

[assembly: XmlConfigurator(ConfigFile = @"agent-log-config.xml", Watch = true)]
namespace TfsConsolePluginRunner
{
    class Program
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
			Log.Info("******************************************************************");
			Log.Info("***************OctaneTfsCConsolePlugin started********************");
			Log.Info("******************************************************************");

            /*if (!ConfigurationManager.ConfigurationExists())
            {
                ConfigurationManager.WriteConfig(ConfigFileGenerator.GenerateConfig());
            }*/

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
