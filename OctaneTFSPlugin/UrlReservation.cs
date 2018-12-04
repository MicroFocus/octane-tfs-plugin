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
using System;
using System.Diagnostics;
using System.Reflection;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Plugin
{
    public static class UrlReservation
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void DoUrlReservation()
        {
            Log.Warn("Starting UrlReservation ...");
            //see all reservations : netsh http show urlacl
            //delete reservation   : netsh http delete urlacl url = http://+:4567/
            //add reservation      : netsh http add urlacl url = http://+:4567/ user=Everyone

            String msg = "";
            String command = "netsh";
            try
            {
                string url = "http://+:4567/";
                String addCommand = $"http add urlacl url={url} user=Everyone";
                Log.Warn("Before add urlacl");
                String output = ExecuteCommand(command, addCommand);
                Log.Warn("After add urlacl");
                if (output.Contains("reservation successfully added"))
                {
                    msg = $"URL reservation for rest server was succesfully added  to {url}";
                    Log.Warn(msg);
                }
                else if (output.Contains("file already exists"))
                {
                    msg = $"URL reservation is already exist to {url}";
                    Log.Warn(msg);
                }
                else
                {
                    msg = $"Error adding url reservation, restserver will not work!. Error = {output}";
                    Log.Error(msg);
                }
            }
            catch (Exception e)
            {
                msg = $"Failed to do UrlReservation : {e.GetType().Name} : {e.Message}";
                Log.Error(msg, e);
            }

        }

        private static string ExecuteCommand(string command, string argument)
        {
            var process = new Process();
            var startInfo =
                new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = command,
                    Arguments = argument,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
            process.StartInfo = startInfo;
            process.Start();

            var output = process.StandardOutput.ReadToEnd();

            return output;
        }
    }
}
