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
using System.Reflection;
using Microsoft.Win32;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity
{
    public enum TfsVersion
    {
        Tfs2015,
        Tfs2017,
        NotDefined,
    }
	public static class Helpers
    {
        public static string GetPluginVersion()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return version;
        }

        public static TfsVersion GetInstalledTfsVersion()
        {
            var rk2015 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\TeamFoundationServer\\14.0\\InstalledComponents\\ApplicationTier", false);
            var rk2017 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\TeamFoundationServer\\15.0\\InstalledComponents\\ApplicationTier", false);

            if (rk2015 != null)
            {
                return TfsVersion.Tfs2015;
            }
            else if(rk2017 != null)
            {
                return TfsVersion.Tfs2017;
            }
            else
            {
                return TfsVersion.NotDefined;
            }

        }
    }

    
}
