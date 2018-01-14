using System.Reflection;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools
{
	public class Helpers
    {
        public static string GetPluginVersion()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return version;
        }
    }
}
