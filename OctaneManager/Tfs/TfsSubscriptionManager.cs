/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.v1.Subscriptions;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs
{
	public class TfsSubscriptionManager
	{
		private readonly TfsRestConnector _tfsConnector;
		public TfsSubscriptionManager(TfsConfiguration tfsConnectionConf)
		{
			_tfsConnector = new TfsRestConnector(tfsConnectionConf);
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
