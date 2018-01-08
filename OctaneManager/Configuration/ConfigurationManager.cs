using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration
{
	public static class ConfigurationManager
	{
		private static string _configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "OctaneTfsPlugin");
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static string _configurationFile = "octane.conf.json";
		private static FileSystemWatcher _watcher;
		private static Task configurationFileChangedTask;

		public static event EventHandler ConfigurationChanged;

		static ConfigurationManager()
		{
			Init();
		}

		public static string ConfigFolder
		{
			get
			{
				return _configFolder;
			}
		}
		public static void Init()
		{
			CreateConfigDirIfMissing();
			WatchForConfigFileChanges();
		}

		public static ConnectionDetails Read()
		{
			var fullConfigFilePath = GetConfigFilePath();
			Log.Debug($"Loading configuration from {fullConfigFilePath}");
			if (!File.Exists(fullConfigFilePath))
			{
				throw new FileNotFoundException($"Configuration file {fullConfigFilePath} was not found!");
			}

			var text = File.ReadAllText(fullConfigFilePath);

			ConnectionDetails resForLog = JsonHelper.DeserializeObject<ConnectionDetails>(text);
			resForLog.RemoveSensitiveInfo();
			Log.Info($"Loaded configuration : {JsonHelper.SerializeObject(resForLog, true)}");

			ConnectionDetails res = JsonHelper.DeserializeObject<ConnectionDetails>(text);
			return res;
		}

		public static bool ConfigurationExists()
		{
			return File.Exists(GetConfigFilePath());
		}

		private static string GetConfigFilePath()
		{
			return Path.Combine(_configFolder, _configurationFile);
		}

		public static void WriteConfig(ConnectionDetails config)
		{
			var configFile = GetConfigFilePath();
			var configText = JsonHelper.SerializeObject(config, true);

			Log.Info($"Writing configuration : {configText}");
			File.WriteAllText(configFile, configText);
			Log.Info($"Writing configuration done");
		}

		private static void CreateConfigDirIfMissing()
		{
			if (!Directory.Exists(_configFolder))
			{
				Directory.CreateDirectory(_configFolder);
			}
		}

		private static void WatchForConfigFileChanges()
		{
			if (_watcher == null)
			{
				// Create a new FileSystemWatcher and set its properties.
				_watcher = new FileSystemWatcher();
				_watcher.Path = _configFolder;
				_watcher.Filter = _configurationFile;
				_watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;

				// Add event handlers.
				_watcher.Changed += ConfigurationFileChanged;
				_watcher.Created += ConfigurationFileChanged;
				_watcher.Deleted += ConfigurationFileChanged;
				_watcher.Renamed += ConfigurationFileChanged;
				_watcher.EnableRaisingEvents = true;
			}
		}

		private static void ConfigurationFileChanged(object sender, FileSystemEventArgs e)
		{
			//We want to delay invoking of event in 5 secs, why?
			//1.FileSystemWatcher may call this method twice
			//2.User click on save configuration more than once
			if (configurationFileChangedTask == null)
			{
				configurationFileChangedTask = Task.Factory.StartNew(() =>
				{
					Thread.Sleep(5000);
					configurationFileChangedTask = null;
					Log.Info($"Configuration changed on FS : {e.ChangeType}");
					ConfigurationChanged?.Invoke(sender, e);
				});
			}
		}
	}
}
