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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.v1.Subscriptions
{
	public class SubscriptionRequest
    {
        public string PublisherId => "tfs";
        public string EventType => "build.complete";
        public string ResourceVersion => "1.0";
        public string ConsumerId => "webHooks";
        public string ConsumerActionId => "httpRequest";

        [JsonProperty]
        protected Dictionary<string, string> PublisherInputs { get; set; } = new Dictionary<string, string>();

        [JsonProperty]
        protected Dictionary<string, string> ConsumerInputs { get; set; } = new Dictionary<string, string>();


        public SubscriptionRequest(string projectId, Uri url)
        {
            AddProject(projectId);
            AddConsumerInput(url);
        }

        public void AddProject(string id)
        {
            PublisherInputs.Add("projectId", id);
        }

        public void AddConsumerInput(Uri url)
        {
            ConsumerInputs.Add("url", url.ToString());
        }

        public string ToJson()
        {
            return JsonHelper.SerializeObject(this);
        }
    }
}
