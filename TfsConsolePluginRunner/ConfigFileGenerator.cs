using System;
using MicroFocus.Ci.Tfs.Octane.Configuration;

namespace TfsConsolePluginRunner
{
    internal class ConfigFileGenerator
    {
        public static ConnectionDetails GenerateConfig()
        {
            var octaneUrl = GetParam("Please enter octane url: ");
            var clientId = GetParam("Please client id: ");
            var clientSecret = GetParam("Please enter client secret: ");
            var tfsLocation = GetParam("Please tfs location: ");
            var pat = GetParam("Please tfs pat: ");


            var config = new ConnectionDetails()
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                WebAppUrl = octaneUrl,
                TfsLocation = tfsLocation,
                Pat = pat
            };

            return config;
        }

        private static string GetParam(string label)
        {
            Console.Write(label);
            return Console.ReadLine();
        }


    }
}
