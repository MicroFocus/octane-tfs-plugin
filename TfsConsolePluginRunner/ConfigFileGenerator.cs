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
using System;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;

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
                ALMOctaneUrl = octaneUrl,
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
