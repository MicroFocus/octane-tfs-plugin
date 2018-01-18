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
using log4net;
using log4net.Config;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using MicroFocus.Ci.Tfs.Octane;
using System;
using System.Reflection;
using System.Threading;

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
