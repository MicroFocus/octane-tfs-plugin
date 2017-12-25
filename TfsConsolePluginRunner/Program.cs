using System;
using System.Reflection;
using log4net;
using log4net.Config;
using MicroFocus.Ci.Tfs.Octane;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using MicroFocus.Ci.Tfs.Octane.Tools;

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

			MicroFocus.Ci.Tfs.Octane.RestServer.Server.GetInstance().Start();
			var _octaneManager = new OctaneManager(PluginRunMode.ConsoleApp);
            _octaneManager.Init();

            Console.WriteLine("TFS plugin is running , press any key to exit...");
            Console.ReadLine();

			MicroFocus.Ci.Tfs.Octane.RestServer.Server.GetInstance().Stop();
			_octaneManager.ShutDown();
            _octaneManager.WaitShutdown();
        }
    }
}
