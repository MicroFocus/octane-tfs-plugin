using System.Web;
using MicroFocus.Ci.Tfs.Octane.Configuration;

namespace MicroFocus.Ci.Tfs.Octane.Tools
{
    internal class UriResolver
    {
        private const string INTERNAL_API = "/internal-api/shared_spaces/";
        private const string ANALYTICS_CI_SERVERS = "/analytics/ci/servers/";
        private const string ANALYTICS_CI_EVENTS = "/analytics/ci/events";

        private const string ANALYTICS_TEST_RESULTS = "/analytics/ci/test-results/";
        
        private readonly int _sharedSpace;
        private readonly InstanceDetails _instDetails;
		private readonly ConnectionDetails _connectionDetails;


		public UriResolver(int sharedSpace,InstanceDetails instDetails, ConnectionDetails connectionDetails)
        {
            _sharedSpace = sharedSpace;
            _instDetails = instDetails;
			_connectionDetails = connectionDetails;
		}

        public string GetTasksUri()
        {
            var baseUri = $"{INTERNAL_API}{_sharedSpace}{ANALYTICS_CI_SERVERS}{_instDetails.InstanceId}/tasks";
            return baseUri;
        }

        public string GetEventsUri()
        {
            var result = $"{INTERNAL_API}{_sharedSpace}{ANALYTICS_CI_EVENTS}";
            return result;
        }

        public string GetTaskQueryParams()
        {
            var result =
                $"self-type={_instDetails.Type}&self-url={HttpUtility.UrlEncode(_instDetails.SelfLocation)}" +
                $"&api-version={_instDetails.ApiVersion}&sdk-version={_instDetails.SdkVersion}" +
                $"&plugin-version={_instDetails.PluginVersion}" +
				$"&client-id={_connectionDetails.ClientId}";
            
            return result;
        }

        public string PostTaskResultUri(string taskId)
        {
            var baseUri = $"{INTERNAL_API}{_sharedSpace}{ANALYTICS_CI_SERVERS}{_instDetails.InstanceId}/tasks";
            var result = $"{baseUri}/{taskId}/result";

            return result;
        }

        public string GetTestResults(bool skipErrors=false)
        {
            var baseUri = $"{INTERNAL_API}{_sharedSpace}{ANALYTICS_TEST_RESULTS}?skip-errors={skipErrors.ToString().ToLower()}";
            return baseUri;
        }
    }
}
