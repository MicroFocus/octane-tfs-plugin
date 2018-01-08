// (c) Copyright 2016 Hewlett Packard Enterprise Development LP
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using Hpe.Nga.Api.Core.Connector;
using Hpe.Nga.Api.Core.Connector.Exceptions;
using log4net;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Connectivity;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Scm;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.TestResults;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.RestServer;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System;
using System.Collections.Generic;
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

		private OctaneApis _octaneApis;
		private TfsApis _tfsApis;
		private readonly ConnectionDetails _connectionConf;

		private readonly int _pollingGetTimeout;
		private Task _taskPollingThread;

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
			if (_octaneApis != null)
			{
				_octaneApis.Disconnect();
			}

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
					string taskDef = null;
					try
					{
						taskDef = _octaneApis.GetTasks(_pollingGetTimeout);
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
						if (!string.IsNullOrEmpty(taskDef))
						{
							Task task = Task.Factory.StartNew(() =>
							{
								if (IsInitialized)
								{
									HandleTask(taskDef);
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
				_octaneApis.SendTaskResult(taskResult.Id.ToString(), taskResult);
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

			_tfsApis = ConnectionCreator.CreateTfsConnection(_connectionConf);
			_octaneApis = ConnectionCreator.CreateOctaneConnection(_connectionConf);

			_taskProcessor = new TaskProcessor(_tfsApis);

			IsInitialized = true;
			InitTaskPolling();//should be after IsInitialized = true
			Log.Debug($"Octane manager initialized successfully");
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
				var list = new List<CiEvent>();
				list.Add(ciEvent);

				bool isFinishEvent = ciEvent.EventType.Equals(CiEventType.Finished);
				if (isFinishEvent)
				{
					var scmData = ScmEventHelper.GetScmData(_tfsApis, ciEvent.BuildInfo);
					if (scmData != null)
					{
						list.Add(CreateScmEvent(ciEvent, scmData));
						Log.Debug($"{ciEvent.BuildInfo} - scm data contains {scmData.Commits.Count} commits");
					}
					else
					{
						Log.Debug($"{ciEvent.BuildInfo} - scm data is empty");
					}
				}


				_octaneApis.SendEvents(list);
				Log.Debug($"{ciEvent.BuildInfo} - {list.Count} events succesfully sent");
				if (isFinishEvent)
				{
					SendTestResults(ciEvent.BuildInfo, ciEvent.Project, ciEvent.BuildId);
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
				if (_octaneApis.IsTestResultRelevant(projectCiId))
				{
					var run = _tfsApis.GetRunForBuid(buildInfo.CollectionName, buildInfo.Project, buildInfo.BuildId);
					if (run == null)
					{
						Log.Debug($"Build {buildInfo} - run was not created for build. No test results");
					}
					else
					{
						var testResults = _tfsApis.GetTestResultsForRun(buildInfo.CollectionName, buildInfo.Project, run.Id.ToString());
						OctaneTestResult octaneTestResult = OctaneUtils.ConvertToOctaneTestResult(_connectionConf.InstanceId.ToString(), projectCiId, buildCiId, testResults, run.WebAccessUrl);
						_octaneApis.SendTestResults(octaneTestResult);

						Log.Debug($"Build {buildInfo} - testResults are sent ({octaneTestResult.TestRuns.Count} tests)");
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
	}
}
