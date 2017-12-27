// (c) Copyright 2016 Hewlett Packard Enterprise Development LP
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using Hpe.Nga.Api.Core.Connector;
using log4net;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Dto.Connectivity;
using MicroFocus.Ci.Tfs.Octane.Dto.Events;
using MicroFocus.Ci.Tfs.Octane.Dto.General;
using MicroFocus.Ci.Tfs.Octane.Dto.Scm;
using MicroFocus.Ci.Tfs.Octane.Dto.TestResults;
using MicroFocus.Ci.Tfs.Octane.RestServer;
using MicroFocus.Ci.Tfs.Octane.Tools;
using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class OctaneManager
	{
		#region Const Definitions

		private const int DEFAULT_POLLING_GET_TIMEOUT = 20 * 1000; //20 seconds
		private const int DEFAULT_POLLING_INTERVAL = 5;

		#endregion

		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly RestConnector _octaneRestConnector = new RestConnector();
		private TfsManager _tfsManager;
		private static ConnectionDetails _connectionConf;

		private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(DEFAULT_POLLING_INTERVAL);
		private readonly int _pollingGetTimeout;
		private Task _taskPollingThread;

		private UriResolver _uriResolver;

		private readonly CancellationTokenSource _pollTasksCancellationToken = new CancellationTokenSource();
		private readonly PluginRunMode _runMode;
		private TaskProcessor _taskProcessor;
		private Uri _tfsServerUri;
		public OctaneManager(PluginRunMode runMode, int pollingTimeout = DEFAULT_POLLING_GET_TIMEOUT)
		{
			_runMode = runMode;
			_pollingGetTimeout = pollingTimeout;

			if (_runMode == PluginRunMode.ConsoleApp)
			{
				RestBase.BuildEvent += RestBase_BuildEvent;
			}

			ConfigurationManager.ConfigurationChanged += OnConfigurationChanged;

			InitTaskPolling();

			Log.Debug("Octane manager created...");
		}



		public void ShutDown()
		{
			_pollTasksCancellationToken.Cancel();
			_octaneRestConnector.Disconnect();
			RestBase.BuildEvent -= RestBase_BuildEvent;
			ConfigurationManager.ConfigurationChanged -= OnConfigurationChanged;
			IsInitialized = false;
            Log.Debug("Octane manager shuted down");
		}

		public void WaitShutdown()
		{
			_taskPollingThread.Wait();
		}

		private void InitTaskPolling()
		{
			Log.Debug("Start init of polling thread");
			_taskPollingThread = Task.Factory.StartNew(() => PollOctaneTasks(_pollTasksCancellationToken.Token), TaskCreationOptions.LongRunning);
		}

		public bool IsInitialized { get; protected set; } = false;

		private void PollOctaneTasks(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				if (_octaneRestConnector.IsConnected())
				{
					ResponseWrapper res = null;
					try
					{
						res = _octaneRestConnector.ExecuteGet(_uriResolver.GetTasksUri(), _uriResolver.GetTaskQueryParams(),
								RequestConfiguration.Create().SetTimeout(_pollingGetTimeout));
					}
					catch (Exception ex)
					{
						if (ex.InnerException != null && ex.InnerException is WebException && ((WebException)ex.InnerException).Status == WebExceptionStatus.Timeout)
						{
							//known exception
						}
						else
						{
							Log.Error($"Task polling exception : {ex.Message}");
						}
					}
					finally
					{
						if (res != null)
						{
							Log.Info($"Received Task:{res?.Data}");

							try
							{
								var octaneTask = JsonHelper.DeserializeObject<OctaneTask>(res?.Data.TrimStart('[').TrimEnd(']'));
								var taskOutput = _taskProcessor.ProcessTask(octaneTask.Method, octaneTask?.ResultUrl);
								int status = HttpMethodEnum.POST.Equals(octaneTask.Method) ? 201 : 200;
								var response = new OctaneTaskResult(status, octaneTask.Id, taskOutput);

								if (octaneTask == null)
								{
									Log.Error("Octane task was not json parsed , Nothing to send....");
								}
								else
								{
									if (!SendTaskResultToOctane(octaneTask.Id, response))
									{
										Log.Error("Error sending results!");
									}
								}

							}
							catch (Exception ex)
							{
								Log.Error("Failed to process task " + ex.Message, ex);
							}
						}


					}
				}
				Thread.Sleep(_pollingInterval);
			}
		}

		private bool SendTaskResultToOctane(Guid resultId, OctaneTaskResult task)
		{
			Log.Debug($"Sending result to octane :  { task.Body}");
			try
			{
				var res = _octaneRestConnector.ExecutePut(_uriResolver.PostTaskResultUri(resultId.ToString()), null, task.ToString());

				if (res.StatusCode == HttpStatusCode.NoContent)
				{
					return true;
				}

				Log.Error($"Unexpected status code during sending task result {resultId} : {res?.StatusCode}");
			}
			catch (Exception ex)
			{
				Log.Error($"Error sending task result {resultId} : {ex.Message}");
			}

			return false;
		}

		private void OnConfigurationChanged(object sender, EventArgs e)
		{
			Init();
		}

		public void Init()
		{
			IsInitialized = false;
			_connectionConf = ConfigurationManager.Read();

			var hostName = Dns.GetHostName();
			var domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
			var tfsServerUriStr = _connectionConf.TfsLocation == null ? $"http://{hostName}.{domainName}:8080/tfs/" : _connectionConf.TfsLocation;
			tfsServerUriStr = tfsServerUriStr.EndsWith("/") ? tfsServerUriStr : tfsServerUriStr + "/";
			_tfsServerUri = new Uri(tfsServerUriStr);


			var instanceDetails = new InstanceDetails(_connectionConf.InstanceId, _tfsServerUri.ToString());
			_uriResolver = new UriResolver(_connectionConf.SharedSpace, instanceDetails, _connectionConf);
			_tfsManager = new TfsManager(_runMode, _tfsServerUri, _connectionConf.Pat);
			_taskProcessor = new TaskProcessor(_tfsManager);

			try
			{
				DateTime start = DateTime.Now;
				Log.Debug($"Validate connection to TFS  {_tfsServerUri.ToString()}");
				//_tfsManager.GetProjectCollections();
				DateTime end = DateTime.Now;
				Log.Debug($"Validate connection to TFS finished in {(long)((end-start).TotalMilliseconds)} ms");
			}
			catch (Exception e)
			{
				var msg = "Invalid connection to TFS :" + e.InnerException != null ? e.InnerException.Message : e.Message;
				Log.Error(msg);
				throw new Exception(msg);
			}

			try
			{
				DateTime start = DateTime.Now;
				Log.Debug($"Validate connection to Octane {_connectionConf.Host}");
				var octaneConnected = _octaneRestConnector.Connect(_connectionConf.Host, new APIKeyConnectionInfo(_connectionConf.ClientId, _connectionConf.ClientSecret));
				DateTime end = DateTime.Now;
				Log.Debug($"Validate connection to Octane finished in {(long)((end - start).TotalMilliseconds)} ms");
			}
			catch (Exception e)
			{
				var msg = "Invalid connection to Octane :" + e.InnerException != null ? e.InnerException.Message : e.Message;
				Log.Error(msg);
				throw new Exception(msg);
			}


			IsInitialized = true;
			Log.Debug($"Initialized successfully");
		}

		private void InitializeConnectionToOctane()
		{
			var connected = _octaneRestConnector.Connect(_connectionConf.Host, new APIKeyConnectionInfo(_connectionConf.ClientId, _connectionConf.ClientSecret));
			if (!connected)
			{
				throw new Exception("Could not connect to octane webapp");
			}
		}

		private void RestBase_BuildEvent(object sender, CiEvent finishEvent)
		{
			if (IsInitialized)
			{
				CiEvent startEvent = CreateStartEvent(finishEvent);
				ReportEventAsync(startEvent).GetAwaiter().OnCompleted(() =>
				{
					ReportEventAsync(finishEvent);

				});
			}
		}

		private CiEvent CreateStartEvent(CiEvent finishEvent)
		{
			var startEvent = finishEvent.Clone();
			startEvent.EventType = CiEventType.Started;

			return startEvent;
		}

		private CiEvent CreateScmEvent(CiEvent finishEvent, ScmData scmData)
		{
			var scmEventEvent = finishEvent.Clone();
			scmEventEvent.EventType = CiEventType.Scm;
			scmEventEvent.ScmData = scmData;
			return scmEventEvent;
		}

		public Task ReportEventAsync(CiEvent ciEvent)
		{
			Task task = Task.Factory.StartNew(() =>
		   {
			   ReportEvent(ciEvent);
		   });
			return task;
		}

		private void ReportEvent(CiEvent ciEvent)
		{
			Log.Debug($"{ciEvent.BuildInfo} - handling {ciEvent.EventType.ToString()} event");

			var list = new CiEventsList();
			list.Events.Add(ciEvent);

			bool isFinishEvent = ciEvent.EventType.Equals(CiEventType.Finished);
			if (isFinishEvent)
			{
				var scmData = ScmEventHelper.GetScmData(_tfsManager, ciEvent.BuildInfo);
				if (scmData != null)
				{
					list.Events.Add(CreateScmEvent(ciEvent, scmData));
					Log.Debug($"{ciEvent.BuildInfo} - scm data contains {scmData.Commits.Count} commits");
				}
				else
				{
					Log.Debug($"{ciEvent.BuildInfo} - scm data is empty");
				}
			}


			list.Server = new CiServerInfo
			{
				Url = _tfsServerUri,
				InstanceId = _connectionConf.InstanceId,
				SendingTime = TestResultUtils.ConvertToOctaneTime(DateTime.UtcNow),
				InstanceIdFrom = TestResultUtils.ConvertToOctaneTime(DateTime.UtcNow)
			};

			var body = JsonHelper.SerializeObject(list);
			var res = _octaneRestConnector.ExecutePut(_uriResolver.GetEventsUri(), null, body);

			if (res.StatusCode == HttpStatusCode.OK)
			{
				Log.Debug($"{ciEvent.BuildInfo} - {list.Events.Count} events succesfully sent");

				if (isFinishEvent)
				{
					SendTestResults(ciEvent.BuildInfo, ciEvent.Project, ciEvent.BuildId);
				}
			}
			else
			{
				Log.Error($"{ciEvent.BuildInfo} - events was not sent succesfully.");
			}
		}

		public void SendTestResults(TfsBuildInfo buildInfo, string projectCiId, string buildCiId)
		{
			try
			{
				if (GetTestResultRelevant(projectCiId))
				{

					var run = _tfsManager.GetRunForBuid(buildInfo.CollectionName, buildInfo.Project, buildInfo.BuildId);
					if (run == null)
					{
						Log.Debug($"{buildInfo} - run didn't create for build. No test results");
					}
					else
					{
						var testResults = _tfsManager.GetTestResultsForRun(buildInfo.CollectionName, buildInfo.Project, run.Id.ToString());
						OctaneTestResult octaneTestResult = TestResultUtils.ConvertToOctaneTestResult(_connectionConf.InstanceId.ToString(), projectCiId, buildCiId, testResults, run.WebAccessUrl);
						String xml = TestResultUtils.SerializeToXml(octaneTestResult);

						ResponseWrapper res = _octaneRestConnector.ExecutePost(_uriResolver.GetTestResults(), null, xml,
							 RequestConfiguration.Create().SetGZipCompression(true).AddHeader("ContentType", "application/xml"));
						Log.Debug($"{buildInfo} - testResults are sent");
					}

				}
				else
				{
					Log.Debug($"{buildInfo} - GetTestResultRelevant=false for project {projectCiId}");
				}
			}
			catch (Exception ex)
			{
				Log.Error($"{buildInfo} : error in SendTestResults : {ex.Message}", ex);
			}
		}

		private bool GetTestResultRelevant(string jobName)
		{
			bool result = false;
			ResponseWrapper res = _octaneRestConnector.ExecuteGet(_uriResolver.GetTestResultRelevant(jobName), null);
			if (res.StatusCode == HttpStatusCode.OK)
			{
				result = Boolean.Parse(res.Data);
			}

			return result;
		}

	}
}
