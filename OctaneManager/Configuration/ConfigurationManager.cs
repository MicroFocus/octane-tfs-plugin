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
        public static string ConfigurationFile => "octane.conf.json";
        public static ConnectionDetails Read()
        {
            
            var fullConfigFilePath = GetConfigFile();

            CheckConfigDir();

            if (!File.Exists(fullConfigFilePath))
            {
                throw new FileNotFoundException($"Configuration file {fullConfigFilePath} was not found!");
            }

            Log.Info("Loading config info...");            

            var file = new FileInfo(fullConfigFilePath);
            var fullFile = file.FullName;
            var sr = new StreamReader(fullFile);
            ConnectionDetails res = null;
            using (var reader = sr)
            {
                var text = reader.ReadToEnd();
                res = JsonHelper.DeserializeObject<ConnectionDetails>(text);
                reader.Close();

                Log.Info(text);
            }            

            Log.Info("Done");
            return res;
        }

        public static bool ConfigurationExists()
        {
            return File.Exists(GetConfigFile());
        }

        public static string GetConfigFile()
        {
            return Path.Combine(_configFolder, ConfigurationFile);

        }

        public static void WriteConfig(ConnectionDetails config)
        {
            CheckConfigDir();
            var configFile = GetConfigFile();
            var configText = JsonHelper.SerializeObject(config);

            Log.Info("Writing configuration info");
            Log.Info(configText);
            File.WriteAllText(configFile, configText);
            Log.Info("Done");
        }

        public static void CheckConfigDir()
        {
            if (!Directory.Exists(_configFolder))
            {
                Directory.CreateDirectory(_configFolder);
            }
        }
    }
}
