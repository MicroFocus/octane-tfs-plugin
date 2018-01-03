using System;

namespace MicroFocus.Ci.Tfs.Octane.Configuration
{
	internal class InstanceDetails
	{
		private const string API_VERSION = "1";
		private const string SDK_VERSION = "1";
		private const string PLUGIN_VERSION = "1";

		public string ApiVersion => API_VERSION;

		public string SdkVersion => SDK_VERSION;

		public string PluginVersion => PLUGIN_VERSION;

		public string Type => "TFS";
		public string InstanceId { get; set; }

		public string SelfLocation { get; protected set; }

		public InstanceDetails(string instanceId, string selfLocation)
		{
			InstanceId = instanceId;
			SelfLocation = selfLocation;
		}

		public InstanceDetails(string selfLocation)
		{
			InstanceId = Guid.NewGuid().ToString();
			SelfLocation = selfLocation;
		}
	}
}
