using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicroFocus.Ci.Tfs.Octane
{
	public static class JsonHelper
	{
		public static T DeserializeObject<T>(string content)
		{
			return JsonConvert.DeserializeObject<T>(content);
		}

		public static string SerializeObject(object value)
		{
			return JsonConvert.SerializeObject(value);
		}

		public static string FormatAsJsonObject(string value)
		{
			return JToken.Parse(value).ToString();
		}



	}
}
