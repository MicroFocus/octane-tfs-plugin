using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools
{
    public class Helpers
    {
        public static string GetPluginVersion()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();


            return $"{assemblyName}<br>{version}";
        }
    }
}
