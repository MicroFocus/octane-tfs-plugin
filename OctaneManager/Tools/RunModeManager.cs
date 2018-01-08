namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools
{
	public class RunModeManager
	{
		private static RunModeManager instance = new RunModeManager();


		public static RunModeManager GetInstance()
		{
			return instance;
		}

		public PluginRunMode RunMode { get; set; } = PluginRunMode.ServerPlugin;

	}
}
