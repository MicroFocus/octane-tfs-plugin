using log4net;
using System;
using System.IO;
using System.Reflection;

namespace MicroFocus.Ci.Tfs.Octane.Configuration
{
	public static class ConfigurationManager
	{
		private static string _configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "OctaneTfsPlugin");
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static string _configurationFile = "octane.conf.json";
		private static FileSystemWatcher _watcher;
		private static DateTime lastWriteTime;

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
			Log.Info("Loading configuration");

			var fullConfigFilePath = GetConfigFilePath();
			if (!File.Exists(fullConfigFilePath))
			{
				throw new FileNotFoundException($"Configuration file {fullConfigFilePath} was not found!");
			}

			var text = File.ReadAllText(fullConfigFilePath);

			ConnectionDetails res = JsonHelper.DeserializeObject<ConnectionDetails>(text);
			Log.Info($"Loaded configuration : {text}");

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
			_watcher.EnableRaisingEvents = false;
			File.WriteAllText(configFile, configText);
			_watcher.EnableRaisingEvents = true;
			Log.Info("Done");
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
			DateTime newLastWriteTime = File.GetLastWriteTime(GetConfigFilePath());
			if (lastWriteTime == null || lastWriteTime != newLastWriteTime)
			{
				Log.Info($"Configuration changed on FS : {e.ChangeType}");
				lastWriteTime = newLastWriteTime;
				ConfigurationChanged?.Invoke(sender, e);
			}
		}





	}
}
