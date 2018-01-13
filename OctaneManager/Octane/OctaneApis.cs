using Hpe.Nga.Api.Core.Connector;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Connectivity;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.General;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.TestResults;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane
{
	public class OctaneApis
	{
		private const string INTERNAL_API = "/internal-api/shared_spaces/";
		private const string ANALYTICS_CI_SERVERS = "/analytics/ci/servers/";
		private const string ANALYTICS_CI_EVENTS = "/analytics/ci/events";

		private const string ANALYTICS_TEST_RESULTS = "/analytics/ci/test-results/";

		private const string API_VERSION = "1";
		private const string SDK_VERSION = "1";
		private string PLUGIN_VERSION = "1";
		private const string PLUGIN_TYPE = "TFS";

		private RestConnector _restConnector;
		private ConnectionDetails _connectionDetails;

		public OctaneApis(RestConnector restConnector, ConnectionDetails connectionDetails)
		{
			_restConnector = restConnector;
			_connectionDetails = connectionDetails;
            PLUGIN_VERSION = Helpers.GetPluginVersion();
        }

		public string PluginInstanceId
		{
			get
			{
				return _connectionDetails.InstanceId;
			}
		}

		public string GetTasks(int pollingTimeout)
		{
			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_CI_SERVERS}{_connectionDetails.InstanceId}/tasks";
			var queryParams =
				$"self-type={PLUGIN_TYPE}&self-url={HttpUtility.UrlEncode(_connectionDetails.TfsLocation)}" +
				$"&api-version={API_VERSION}&sdk-version={SDK_VERSION}" +
				$"&plugin-version={PLUGIN_VERSION}" +
				$"&client-id={_connectionDetails.ClientId}";
			ResponseWrapper wrapper = _restConnector.ExecuteGet(baseUri, queryParams, RequestConfiguration.Create().SetTimeout(pollingTimeout));
			return wrapper.Data;
		}

		public void SendTaskResult(string taskId, OctaneTaskResult octaneTaskResult)
		{
			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_CI_SERVERS}{_connectionDetails.InstanceId}/tasks/{taskId}/result";
			ResponseWrapper res = _restConnector.ExecutePut(baseUri, null, octaneTaskResult.ToString());
			ValidateExpectedStatusCode(res, HttpStatusCode.NoContent);
		}

		public bool IsTestResultRelevant(string jobName)
		{
			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_CI_SERVERS}{_connectionDetails.InstanceId}/jobs/{jobName}/tests-result-preflight";
			bool result = false;
			ResponseWrapper res = _restConnector.ExecuteGet(baseUri, null);
			if (res.StatusCode == HttpStatusCode.OK)
			{
				result = Boolean.Parse(res.Data);
			}
			return result;
		}

		public void SendEvents(IList<CiEvent> list)
		{
			var eventList = new CiEventsList();
			eventList.Events.AddRange(list);
			eventList.Server = new CiServerInfo
			{
				Url = _connectionDetails.TfsLocation,
				InstanceId = _connectionDetails.InstanceId,
				SendingTime = OctaneUtils.ConvertToOctaneTime(DateTime.UtcNow),
				InstanceIdFrom = OctaneUtils.ConvertToOctaneTime(DateTime.UtcNow)
			};

			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_CI_EVENTS}";
			var body = JsonHelper.SerializeObject(eventList);
			var res = _restConnector.ExecutePut(baseUri, null, body);
			ValidateExpectedStatusCode(res, HttpStatusCode.OK);
		}

		private void ValidateExpectedStatusCode(ResponseWrapper res, HttpStatusCode expectedStatus)
		{
			if (res.StatusCode != expectedStatus)
			{
				throw new Exception($"Unexpected status code {res.StatusCode}. Expected status is {expectedStatus}");
			}
		}

		public void SendTestResults(OctaneTestResult octaneTestResult)
		{
			bool skipErrors = false;
			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_TEST_RESULTS}?skip-errors={skipErrors.ToString().ToLower()}";

			String xml = OctaneUtils.SerializeToXml(octaneTestResult);
			ResponseWrapper res = _restConnector.ExecutePost(baseUri, null, xml,
						 RequestConfiguration.Create().SetGZipCompression(true).AddHeader("ContentType", "application/xml"));

			ValidateExpectedStatusCode(res, HttpStatusCode.Accepted);
		}

	}
}
