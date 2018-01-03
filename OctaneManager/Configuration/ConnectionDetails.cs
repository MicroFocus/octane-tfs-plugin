using System;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Configuration
{
	public class ConnectionDetails
	{
		[JsonIgnore]
		public string Host
		{
			get
			{
				Uri uri = new Uri(WebAppUrl);
				string host = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
				return host;
			}
		}

		public int SharedSpace
		{
			get
			{
				try
				{
					var index = WebAppUrl.IndexOf("=", StringComparison.Ordinal);
					var sharedSpaceStr = WebAppUrl.Substring(index + 1);
					var endIndex = sharedSpaceStr.IndexOf("/", StringComparison.Ordinal);
					if (endIndex > -1)
					{
						sharedSpaceStr = sharedSpaceStr.Substring(0, endIndex);
					}
					var res = int.Parse(sharedSpaceStr);
					return res;
				}
				catch (Exception)
				{
					throw new Exception($"Sharedspace id is expected, but not received in {WebAppUrl}");
				}
			}
		}

		public string WebAppUrl { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }

		public string Pat { get; set; }

		public string TfsLocation { get; set; }
		public ConnectionDetails()
		{

		}

		public ConnectionDetails(string webAppUrl, string clientId, string clientSecret, string tfsLocation, string instanceId)
		{
			WebAppUrl = webAppUrl;
			ClientId = clientId;
			ClientSecret = clientSecret;
			InstanceId = (instanceId == null ? Guid.NewGuid().ToString() : instanceId);
			TfsLocation = tfsLocation;
		}

		public string InstanceId { get; set; } = Guid.NewGuid().ToString();

		public ConnectionDetails RemoveSensitiveInfo()
		{
			ClientSecret = "***";
			Pat = "***";
			return this;
		}

	}
}
