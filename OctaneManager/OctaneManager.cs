// (c) Copyright 2016 Hewlett Packard Enterprise Development LP
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using Hpe.Nga.Api.Core.Connector;
using Hpe.Nga.Api.Core.Connector.Exceptions;
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
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class OctaneManager
	{
		#region Const Definitions

		private const int DEFAULT_POLLING_GET_TIMEOUT = 20 * 1000; //20 seconds

		#endregion

		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private RestConnector _octaneRestConnector;
		private TfsManager _tfsManager;
		private readonly ConnectionDetails _connectionConf;

		private readonly int _pollingGetTimeout;
		private Task _taskPollingThread;

		private UriResolver _uriResolver;

		private readonly CancellationTokenSource _pollTasksCancellationToken = new CancellationTokenSource();
		private TaskProcessor _taskProcessor;

		public OctaneManager(ConnectionDetails connectionDetails, int pollingTimeout = DEFAULT_POLLING_GET_TIMEOUT)
		{
			_connectionConf = connectionDetails;
			_pollingGetTimeout = pollingTimeout;

			if (RunModeManager.GetInstance().RunMode == PluginRunMode.ConsoleApp)
			{
				RestBase.BuildEvent += RestBase_BuildEvent;
			}

			Log.Debug("Octane manager created...");
		}

		public void ShutDown()
		{
			IsInitialized = false;
			_pollTasksCancellationToken.Cancel();
			if (_octaneRestConnector != null)
			{
				_octaneRestConnector.Disconnect();
			}
			_octaneRestConnector = null;
			RestBase.BuildEvent -= RestBase_BuildEvent;
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
			Log.Debug("Task polling - started");
			while (!token.IsCancellationRequested)
			{
				if (IsInitialized)
				{
					ResponseWrapper res = null;
					try
					{
						res = _octaneRestConnector.ExecuteGet(_uriResolver.GetTasksUri(), _uriResolver.GetTaskQueryParams(),
								RequestConfiguration.Create().SetTimeout(_pollingGetTimeout));
					}
					catch (Exception ex)
					{
						Exception myEx = ex;
						if (ex is AggregateException && ex.InnerException != null)
						{
							myEx = ex.InnerException;
						}
						if (myEx is WebException && ((WebException)myEx).Status == WebExceptionStatus.Timeout)
						{
							//known exception
							//Log.Debug($"Task polling - no task received");
						}
						else if (myEx is ServerUnavailableException)
						{
							Log.Error($"Octane server is unavailable");
							OctaneManagerInitializer.GetInstance().RestartPlugin();
						}
						else
						{
							Log.Error($"Task polling exception : {myEx.Message}");
							Thread.Sleep(DEFAULT_POLLING_GET_TIMEOUT);//wait before next pool
						}
					}
					finally
					{
						if (res != null && res.Data != null)
						{
							Task task = Task.Factory.StartNew(() =>
							{
								if (IsInitialized)
								{
									HandleTask(res.Data);
								}
							});
						}
					}
				}
			}
			Log.Debug("Task polling - finished");
		}

		private void HandleTask(string taskData)
		{
			Log.Info($"Received Task:{taskData}");
			try
			{
				//process task
				var octaneTask = JsonHelper.DeserializeObject<OctaneTask>(taskData.TrimStart('[').TrimEnd(']'));
				if (octaneTask == null)
				{
					Log.Error("Octane task was not json parsed , nothing to handle....");
					return;
				}
				var start = DateTime.Now;
				var taskOutput = _taskProcessor.ProcessTask(octaneTask.Method, octaneTask?.ResultUrl);
				var end = DateTime.Now;
				Log.Debug($"Task processed in {(long)((end - start).TotalMilliseconds)} ms");

				//prepare response
				int status = HttpMethodEnum.POST.Equals(octaneTask.Method) ? 201 : 200;
				var taskResult = new OctaneTaskResult(status, octaneTask.Id, taskOutput);
				Log.Debug($"Sending result to octane :  { taskResult.Body}");
				_octaneRestConnector.ExecutePut(_uriResolver.PostTaskResultUri(taskResult.Id.ToString()), null, taskResult.ToString());
			}
			catch (InvalidCredentialException ex)
			{
				Log.Error("Failed to process task : " + ex.Message);
				OctaneManagerInitializer.GetInstance().RestartPlugin();
			}
			catch (Exception ex)
			{
				Log.Error("Failed to process task " + ex.Message, ex);
			}
		}

		public void Init()
		{
			IsInitialized = false;

			_tfsManager = ConnectionCreator.CreateTfsConnection(_connectionConf);
			_octaneRestConnector = ConnectionCreator.CreateOctaneConnection(_connectionConf);

			var instanceDetails = new InstanceDetails(_connectionConf.InstanceId, _tfsManager.TfsUri.ToString());
			_uriResolver = new UriResolver(_connectionConf.SharedSpace, instanceDetails, _connectionConf);
			_taskProcessor = new TaskProcessor(_tfsManager);
			
			IsInitialized = true;
			InitTaskPolling();//should be after IsInitialized = true
			Log.Debug($"Octane manager initialized successfully");
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
			try
			{
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
					Url = _tfsManager.TfsUri,
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
			catch (InvalidCredentialException e)
			{
				Log.Error($"ReportEvent failed with TFS : {e.Message}");
				OctaneManagerInitializer.GetInstance().RestartPlugin();
			}
			catch (Exception e)
			{
				Log.Error($"ReportEvent failed : {e.Message}", e);
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
						Log.Debug($"Build {buildInfo} - run was not created for build. No test results");
					}
					else
					{
						var testResults = _tfsManager.GetTestResultsForRun(buildInfo.CollectionName, buildInfo.Project, run.Id.ToString());
						OctaneTestResult octaneTestResult = TestResultUtils.ConvertToOctaneTestResult(_connectionConf.InstanceId.ToString(), projectCiId, buildCiId, testResults, run.WebAccessUrl);
						String xml = TestResultUtils.SerializeToXml(octaneTestResult);

						ResponseWrapper res = _octaneRestConnector.ExecutePost(_uriResolver.GetTestResults(), null, xml,
							 RequestConfiguration.Create().SetGZipCompression(true).AddHeader("ContentType", "application/xml"));
						Log.Debug($"Build {buildInfo} - testResults are sent ({octaneTestResult.TestRuns.Count} runs)");
					}

				}
				else
				{
					Log.Debug($"Build {buildInfo} - GetTestResultRelevant=false for project {projectCiId}");
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Build {buildInfo} : error in SendTestResults : {ex.Message}", ex);
				throw ex;
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
