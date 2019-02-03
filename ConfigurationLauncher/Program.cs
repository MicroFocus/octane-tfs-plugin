using log4net;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;

namespace ConfigurationLauncher
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string CONFIGURATION_URL = "http://localhost:4567/config";

        static void Main(string[] args)
        {
            LogUtils.ConfigureLog4NetForSetup();
            StartConfigurator();
        }

        private static void StartConfigurator()
        {
            DateTime start = DateTime.Now;
            Thread.Sleep(5000);//initial wait
            Log.Warn($"StartConfigurator at {CONFIGURATION_URL}");

            //Wait before start.
            //this method is called after service turned to status "running" 
            //but still required some time to application to start all components
            bool configurationUp = false;
            int counter = 0;
            int MAX_COUNTER = 15;
            int PAUSE_TIME = 2000;
            

            while (!configurationUp && counter < MAX_COUNTER)
            {
                Thread.Sleep(PAUSE_TIME);
                counter++;
                Log.Warn("configuration page ping counter=" + counter);
                try
                {
                    Uri urlCheck = new Uri(CONFIGURATION_URL);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlCheck);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Log.Warn("Configuration page response status " + response.StatusCode);
                    configurationUp = response.StatusCode == HttpStatusCode.OK;
                }
                catch (Exception)
                {
                    //Log.Warn("Ping failed " + e.Message);
                }
            }

            TimeSpan span = DateTime.Now.Subtract(start);
            Log.Warn($"StartConfigurator done in {span.TotalSeconds} seconds");

            Process.Start(CONFIGURATION_URL);
        }
    }
}
