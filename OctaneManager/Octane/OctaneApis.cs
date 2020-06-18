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
using log4net;
using MicroFocus.Adm.Octane.Api.Core.Connector;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Connectivity;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.General;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.TestResults;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane
{
	public class OctaneApis
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private const string INTERNAL_API = "/internal-api/shared_spaces/";
		private const string ANALYTICS_CI_SERVERS = "/analytics/ci/servers/";
		private const string ANALYTICS_CI_EVENTS = "/analytics/ci/events";

		private const string ANALYTICS_TEST_RESULTS = "/analytics/ci/test-results/";

		private const string API_VERSION = "1";
		private const string SDK_VERSION = "1";
		private string PLUGIN_VERSION = "1";
		private const string PLUGIN_TYPE = "tfs";

		private RestConnector _restConnector;
		private ConnectionDetails _connectionDetails;
		private int DEFAULT_TIMEOUT = 60 * 1000; //60 seconds;
		private int TASK_POLLING_TIMEOUT = 30 * 1000; //30 seconds;
		private Task requestWatcher;
		private readonly CancellationTokenSource _requestWatcherTaskCancellationToken = new CancellationTokenSource();

		public OctaneApis(RestConnector restConnector, ConnectionDetails connectionDetails)
		{
			_restConnector = restConnector;
			_connectionDetails = connectionDetails;
			PLUGIN_VERSION = Helpers.GetPluginVersion();
			requestWatcher = Task.Factory.StartNew(() => WatchRequests(_requestWatcherTaskCancellationToken.Token), TaskCreationOptions.LongRunning);
		}

		public void ShutDown()
		{
			if (!_requestWatcherTaskCancellationToken.IsCancellationRequested)
			{
				_requestWatcherTaskCancellationToken.Cancel();
				Log.Debug("OctaneApis - stopped");
			}
		}

		private void WatchRequests(CancellationToken token)
		{
			Log.Debug("WatchRequests - started");
			while (!token.IsCancellationRequested)
			{
				try
				{
					IList<OngoingRequest> ongoingRequests = _restConnector.GetOngoingRequests();
					foreach (OngoingRequest ongoing in ongoingRequests)
					{
						int duration = (int)DateTime.Now.Subtract(ongoing.Started).TotalMilliseconds;
						if (duration > ongoing.Request.Timeout)
						{
							Log.Warn($"Closing request after  {duration/1000} sec : { ongoing.Request.RequestUri}");
							ongoing.Request.Abort();
						}
					}
				}
				catch (Exception e)
				{
					Log.Error("Exception in WatchRequests : " + e.Message, e);
				}

				Thread.Sleep(10000);
			}
			Log.Debug("WatchRequests - finished");
		}


		public string PluginInstanceId
		{
			get
			{
				return _connectionDetails.InstanceId;
			}
		}

		public string GetTasks()
		{
			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_CI_SERVERS}{_connectionDetails.InstanceId}/tasks";
			var queryParams =
				$"self-type={PLUGIN_TYPE}&self-url={HttpUtility.UrlEncode(_connectionDetails.TfsLocation.TrimEnd('/'))}" +
				$"&api-version={API_VERSION}&sdk-version={SDK_VERSION}" +
				$"&plugin-version={PLUGIN_VERSION}" +
				$"&client-id={_connectionDetails.ClientId}";
			ResponseWrapper wrapper = _restConnector.ExecuteGet(baseUri, queryParams, RequestConfiguration.Create().SetTimeout(TASK_POLLING_TIMEOUT)); 
			return wrapper.Data;
		}

		public void SendTaskResult(string taskId, OctaneTaskResult octaneTaskResult)
		{
			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_CI_SERVERS}{_connectionDetails.InstanceId}/tasks/{taskId}/result";
			ResponseWrapper res = _restConnector.ExecutePut(baseUri, null, octaneTaskResult.ToString(), RequestConfiguration.Create().SetTimeout(DEFAULT_TIMEOUT));
			ValidateExpectedStatusCode(res, HttpStatusCode.NoContent);
		}

		public bool IsTestResultRelevant(string jobName)
		{
			var baseUri = $"{INTERNAL_API}{_connectionDetails.SharedSpace}{ANALYTICS_CI_SERVERS}{_connectionDetails.InstanceId}/jobs/{jobName}/tests-result-preflight";
			bool result = false;
			ResponseWrapper res = _restConnector.ExecuteGet(baseUri, null, RequestConfiguration.Create().SetTimeout(DEFAULT_TIMEOUT));
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
			var res = _restConnector.ExecutePut(baseUri, null, body, RequestConfiguration.Create().SetTimeout(DEFAULT_TIMEOUT));
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

			String xml = OctaneTestResutsUtils.SerializeToXml(octaneTestResult);
			ResponseWrapper res = _restConnector.ExecutePost(baseUri, null, xml,
						 RequestConfiguration.Create()
						 .SetGZipCompression(true)
						 .AddHeader("ContentType", "application/xml")
						 .SetTimeout(DEFAULT_TIMEOUT));

			ValidateExpectedStatusCode(res, HttpStatusCode.Accepted);
		}

	}
}
