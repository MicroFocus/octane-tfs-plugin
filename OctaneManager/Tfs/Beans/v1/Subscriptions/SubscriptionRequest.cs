using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1.Subscriptions
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
