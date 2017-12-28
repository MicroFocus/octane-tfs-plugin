using System;
using System.Reflection;
using log4net;
using log4net.Config;
using MicroFocus.Ci.Tfs.Octane;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using MicroFocus.Ci.Tfs.Octane.Tools;
using System.Threading;

[assembly: XmlConfigurator(ConfigFile = @"agent-log-config.xml", Watch = true)]
namespace TfsConsolePluginRunner
{
    class Program
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {            
            Log.Info("Starting proccess...");

            if (!ConfigurationManager.ConfigurationExists())
            {
                ConfigurationManager.WriteConfig(ConfigFileGenerator.GenerateConfig());
            }

			OctaneManagerInitializer octaneManagerInitializer = OctaneManagerInitializer.GetInstance();
			octaneManagerInitializer.RunMode = PluginRunMode.ConsoleApp;
			octaneManagerInitializer.Start();
			Console.WriteLine("TFS plugin is running , press any key to exit...");
            Console.ReadLine();

			octaneManagerInitializer.Stop();
			Console.WriteLine("TFS plugin is stopped. The window will close shortly.");
			Thread.Sleep(5000);
		}
    }
}
