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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Connectivity;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MicroFocus.Ci.Tfs.Octane
{
    public class OctaneTaskManager
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected static readonly ILog LogPolling = LogManager.GetLogger(LogUtils.TASK_POLLING_LOGGER);


        private const int DEFAULT_POLLING_GET_TIMEOUT = 20 * 1000; //20 seconds

		private readonly int _pollingGetTimeout;
		private Task _taskPollingThread;

		private readonly CancellationTokenSource _pollTasksCancellationToken = new CancellationTokenSource();

		private readonly TfsApis _tfsApis;
		private readonly OctaneApis _octaneApis;

		private enum TaskType
		{
			GetJobsList,
			GetJobDetail,
			ExecutePipelineRunRequest,
			Undefined
		}

		public OctaneTaskManager(TfsApis tfsApis, OctaneApis octaneApis, int pollingTimeout = DEFAULT_POLLING_GET_TIMEOUT)
		{
			_tfsApis = tfsApis;
			_octaneApis = octaneApis;
			_pollingGetTimeout = pollingTimeout;
		}

		public void ShutDown()
		{
			if (!_pollTasksCancellationToken.IsCancellationRequested)
			{
				_pollTasksCancellationToken.Cancel();
				Log.Debug("OctaneTaskManager - stopped");
			}
		}

		public void WaitShutdown()
		{
			_taskPollingThread.Wait();
		}

		public void Start()
		{
			_taskPollingThread = Task.Factory.StartNew(() => PollOctaneTasks(_pollTasksCancellationToken.Token), TaskCreationOptions.LongRunning);
		}

		private void PollOctaneTasks(CancellationToken token)
		{
			Log.Debug("Task polling - started");
            LogPolling.Debug("Task polling - started");
            while (!token.IsCancellationRequested)
			{
				string taskDef = null;
				try
				{
                    LogPolling.Debug("Task polling - before get task");
                    taskDef = _octaneApis.GetTasks(_pollingGetTimeout);
					if (!string.IsNullOrEmpty(taskDef))
					{
						Task.Factory.StartNew(() =>
						{
							HandleTasks(taskDef);
						});
					}
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
                        LogPolling.Debug($"Task polling - no task received");
					}
					else
					{
						if (!ExceptionHelper.HandleExceptionAndRestartIfRequired(myEx, Log, "Task polling"))
						{
							Thread.Sleep(DEFAULT_POLLING_GET_TIMEOUT);//wait before next pool
						}
					}
				}
			}
			Log.Debug("Task polling - finished");
            LogPolling.Debug("Task polling - finished");
        }


		private void HandleTasks(string taskData)
		{
			Log.Info($"Received tasks : {taskData}");
            LogPolling.Debug($"Received tasks : {taskData}");
            OctaneTaskResult taskResult = null;
			OctaneTask octaneTask = null;
			try
			{
				//process task
				var octaneTasks = JsonHelper.DeserializeObject<List<OctaneTask>>(taskData);

				if (octaneTasks == null || octaneTasks.Count == 0)
				{
					Log.Error("Received data does not contains any valid task.");
					return;
				}
				octaneTask = octaneTasks[0];

				int status = HttpMethodEnum.POST.Equals(octaneTask.Method) ? 201 : 200;
				taskResult = new OctaneTaskResult(status, octaneTask.Id, "");
				ExecuteTask(octaneTask?.ResultUrl, taskResult);
				Log.Debug($"Sending result to octane :  {taskResult.Status} - {taskResult.Body}");
			}
			catch (Exception ex)
			{

				if (octaneTask != null)
				{
					taskResult.Status = 500;
					taskResult.Body = "failed to process";
				}


				ExceptionHelper.HandleExceptionAndRestartIfRequired(ex, Log, "HandleTask");

			}
			finally
			{
				if (taskResult != null)
				{
					_octaneApis.SendTaskResult(taskResult.Id.ToString(), taskResult);
				}

			}
		}

		private void ExecuteTask(Uri taskUrl, OctaneTaskResult taskResult)
		{
			var start = DateTime.Now;
			var taskType = ParseTaskUriToTaskType(taskUrl);
			switch (taskType)
			{
				case TaskType.GetJobsList:
					taskResult.Body = JsonHelper.SerializeObject(_tfsApis.GetJobsList());
					break;
				case TaskType.GetJobDetail:
					var jobId = taskUrl.Segments[taskUrl.Segments.Length - 1];
					taskResult.Body = JsonHelper.SerializeObject(_tfsApis.GetJobDetail(jobId));
					break;
				case TaskType.ExecutePipelineRunRequest:
					var joinedProjectName = taskUrl.Segments[taskUrl.Segments.Length - 2].Trim('/');
					var buildParts = joinedProjectName.Split('.');
					QueueNewBuild(taskResult, buildParts[0], buildParts[1], buildParts[2]);
					break;
				case TaskType.Undefined:
					Log.Debug($"Undefined task : {taskUrl}");
					break;
				default:
					break;
			}

			var end = DateTime.Now;
			Log.Debug($"Task {taskType} executed in {(long)((end - start).TotalMilliseconds)} ms");
		}

		private void QueueNewBuild(OctaneTaskResult taskResult, string collectionName, string projectId, string buildDefinitionId)
		{
			try
			{
				_tfsApis.QueueNewBuild(collectionName, projectId, buildDefinitionId);
				taskResult.Body = "Job started";
			}
			catch (Exception e)
			{
				Log.Error("Failed to QueueNewBuild :" + e.Message, e);
				if (e is UnauthorizedAccessException)
				{
					taskResult.Status = 403;//qc:pipeline-management-run-pipeline-failed-no-permission
					taskResult.Body = "No permissions";
				}
				else if (e.Message.Contains("BuildRequestValidationFailedException") && e.Message.Contains("Could not queue the build"))
				{
					taskResult.Status = 503;//qc:pipeline-management-ci-is-down
					taskResult.Body = "Agent is down";
				}
				else
				{
					taskResult.Status = 500;
					taskResult.Body = "Failed to queue job";
				}
			}
		}

		private static TaskType ParseTaskUriToTaskType(Uri uriPath)
		{
			var uri = uriPath;
			var uriPathList = uriPath.Segments.ToList();
			for (var i = 0; i < uriPathList.Count; i++)
			{
				uriPathList[i] = uriPathList[i].Replace("/", "");
			}

			var octaneParams = uriPathList.GetRange(uriPathList.IndexOf("v1") + 1, uriPathList.Count - uriPathList.IndexOf("v1") - 1);

			var param = octaneParams[0];
			if (param == "jobs")
			{
				if (octaneParams.Count == 1)
				{
					if (HttpUtility.ParseQueryString(uri.Query)["parameters"] == "true")
					{
						return TaskType.GetJobsList;
					}
					return TaskType.GetJobsList;
				}
				else if (octaneParams.Count == 2)
				{
					return TaskType.GetJobDetail;
				}
				else if (octaneParams.Count == 3 && octaneParams[2].Equals("run"))
				{
					return TaskType.ExecutePipelineRunRequest;
				}
			}

			Log.Debug($"Found undefined taskUrl type `{param}` in uri: {uriPath}");

			return TaskType.Undefined;

		}
	}
}
