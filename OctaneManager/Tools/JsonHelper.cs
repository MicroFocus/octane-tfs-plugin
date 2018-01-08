using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools
{
	public static class JsonHelper
	{
		public static T DeserializeObject<T>(string content)
		{
			return JsonConvert.DeserializeObject<T>(content);
		}

		public static string SerializeObject(object value)
		{
			return SerializeObject(value, false);
		}

		public static string SerializeObject(object value, bool formatted)
		{
			return JsonConvert.SerializeObject(value, formatted ? Formatting.Indented : Formatting.None);
		}

		public static string FormatAsJsonObject(string value)
		{
			return JToken.Parse(value).ToString();
		}
	}
}
