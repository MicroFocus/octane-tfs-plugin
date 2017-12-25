using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1.Subscriptions;
using MicroFocus.Ci.Tfs.Octane.Tools;
using System;

namespace MicroFocus.Ci.Tfs.Octane.Tfs
{
	public class SubscriptionManager
	{
		private readonly TfsHttpConnector _tfsConnector;
		public SubscriptionManager(TfsConfiguration tfsConnectionConf)
		{
			_tfsConnector = new TfsHttpConnector(tfsConnectionConf);
		}

		public void AddSubscription(string collectionName, string projectId)
		{
			//TODO: handle error
			var subscription = new SubscriptionRequest(projectId, new Uri("http://localhost:4567/build-event"));
			var uriSuffix = ($"{collectionName}/_apis/hooks/subscriptions/?api-version=1.0");

			_tfsConnector.SendPost<Object>(uriSuffix, subscription.ToJson());
		}

		public bool SubscriptionExists(string collectionName, string projectId)
		{
			var uriSuffix = ($"{collectionName}/_apis/hooks/subscriptions/?api-version=1.0");
			var res = _tfsConnector.SendGet<SubscriptionsListResponse>(uriSuffix);

			var str = JsonHelper.SerializeObject(res.Value);
			return str.Contains(projectId); //TODO:check if this is bullet proof
		}


	}
}
