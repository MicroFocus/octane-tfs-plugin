using Newtonsoft.Json;
using System;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration
{
	public class ConnectionDetails
	{
		[JsonIgnore]
		public string Host
		{
			get
			{
				Uri uri = new Uri(ALMOctaneUrl);
				string host = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
				return host;
			}
		}

		[JsonIgnore]
		public int SharedSpace
		{
			get
			{
				try
				{
					var index = ALMOctaneUrl.IndexOf("=", StringComparison.Ordinal);
					var sharedSpaceStr = ALMOctaneUrl.Substring(index + 1);
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
					throw new Exception($"Sharedspace id is expected, but not received in {ALMOctaneUrl}");
				}
			}
		}
		public string ALMOctaneUrl { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }

		public string Pat { get; set; }

		public string TfsLocation { get; set; }
		public ConnectionDetails()
		{

		}

		public ConnectionDetails(string octaneUrl, string clientId, string clientSecret, string tfsLocation, string instanceId)
		{
			ALMOctaneUrl = octaneUrl;
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
