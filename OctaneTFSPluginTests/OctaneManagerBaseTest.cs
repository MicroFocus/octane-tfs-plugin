﻿using System;
using System.IO;
using MicroFocus.Ci.Tfs.Octane;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace MicroFocus.Ci.Tfs.Tests
{
    [TestClass]
    public class OctaneManagerBaseTest
    {
        protected static OctaneManager octaneManager;
        [AssemblyInitialize]
        public static void InitializeConfigs(TestContext context)
        {
            var webbAppUrl = ConfigurationManager.AppSettings["webAppUrl"];
            var clientId = ConfigurationManager.AppSettings["clientId"];
            var clientSecret = ConfigurationManager.AppSettings["clientSecret"];
            var devTimeout = (int)TimeSpan.FromSeconds(int.Parse(ConfigurationManager.AppSettings["devTimeout"])).TotalMilliseconds;
            var instanceId = Guid.Parse(ConfigurationManager.AppSettings["InstanceId"]);

            var path = MicroFocus.Ci.Tfs.Octane.Configuration.ConfigurationManager.ConfigurationFile;

            var connectionDetails = new ConnectionDetails(webbAppUrl,clientId,clientSecret,instanceId);
            using (TextWriter writer = new StreamWriter(path))
            {
                var config = JsonConvert.SerializeObject(connectionDetails);
                writer.Write(config);
                writer.Close();
            }     
            
            octaneManager = new OctaneManager(8080,devTimeout);
        }
    }
}