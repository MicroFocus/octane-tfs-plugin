using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Tools
{
    public class Helpers
    {
        public static string GetPluginVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();            
        }
    }
}
