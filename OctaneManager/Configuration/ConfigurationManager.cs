using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Configuration
{
    internal static class ConfigurationManager
    {
        public static string ConfigurationFile => @"c:\temp\octane.conf.json";
        public static ConnectionDetails Read()
        {
            string currentDir = Environment.CurrentDirectory;
            DirectoryInfo directory = new DirectoryInfo(currentDir);
            FileInfo file = new FileInfo(ConfigurationFile);

            string fullDirectory = directory.FullName;
            string fullFile = file.FullName;

            StreamReader sr = new StreamReader(fullFile);

            ConnectionDetails res = null;
            using (var reader = sr)
            {
                res = JsonConvert.DeserializeObject<ConnectionDetails>(reader.ReadToEnd());
                reader.Close();
            }

            

            return res;
        }                
    }
}
