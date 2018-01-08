using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using System.Web;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane
{
	internal class OctaneUriResolver
	{
		private const string INTERNAL_API = "/internal-api/shared_spaces/";
		private const string ANALYTICS_CI_SERVERS = "/analytics/ci/servers/";
		private const string ANALYTICS_CI_EVENTS = "/analytics/ci/events";

		private const string ANALYTICS_TEST_RESULTS = "/analytics/ci/test-results/";

		private readonly ConnectionDetails _connectionDetails;

		private const string API_VERSION = "1";
		private const string SDK_VERSION = "1";
		private const string PLUGIN_VERSION = "1";
		private const string PLUGIN_TYPE = "TFS";


		public OctaneUriResolver(ConnectionDetails connectionDetails)
		{
			_connectionDetails = connectionDetails;
		}

		public string GetTasksUri()
		{
			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_CI_SERVERS}{_connectionDetails.InstanceId}/tasks";
			return baseUri;
		}

		public string GetEventsUri()
		{
			var result = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_CI_EVENTS}";
			return result;
		}

		public string GetTaskQueryParams()
		{
			var result =
				$"self-type={PLUGIN_TYPE}&self-url={HttpUtility.UrlEncode(_connectionDetails.TfsLocation)}" +
				$"&api-version={API_VERSION}&sdk-version={SDK_VERSION}" +
				$"&plugin-version={PLUGIN_VERSION}" +
				$"&client-id={_connectionDetails.ClientId}";

			return result;
		}

		public string PostTaskResultUri(string taskId)
		{
			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_CI_SERVERS}{_connectionDetails.InstanceId}/tasks";
			var result = $"{baseUri}/{taskId}/result";

			return result;
		}

		public string GetTestResults(bool skipErrors = false)
		{
			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_TEST_RESULTS}?skip-errors={skipErrors.ToString().ToLower()}";
			return baseUri;
		}

		public string GetTestResultRelevant(string jobName)
		{
			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_CI_SERVERS}{_connectionDetails.InstanceId}/jobs/{jobName}/tests-result-preflight";
			return baseUri;
		}
	}
}
