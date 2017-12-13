using System;
using System.Reflection;
using log4net;
using log4net.Config;
using MicroFocus.Ci.Tfs.Octane;
using MicroFocus.Ci.Tfs.Octane.Configuration;

[assembly: XmlConfigurator(ConfigFile = @"agent-log-config.xml", Watch = true)]
namespace TfsConsolePluginRunner
{
    class Program
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //private static readonly TfsManager _tfsManager = new TfsManager();
        private static OctaneManager _octaneManager;

        static void Main(string[] args)
        {            
            Log.Info("Starting proccess...");

            if (!ConfigurationManager.ConfigurationExists())
            {
                ConfigurationManager.WriteConfig(ConfigFileGenerator.GenerateConfig());
            }

            _octaneManager = new OctaneManager();
            _octaneManager.Init();

            Console.WriteLine("TFS plugin is running , press any key to exit...");
            Console.ReadLine();
            _octaneManager.ShutDown();
            _octaneManager.WaitShutdown();
        }
    }
}
