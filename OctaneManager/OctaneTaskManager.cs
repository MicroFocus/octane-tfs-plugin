using Hpe.Nga.Api.Core.Connector.Exceptions;
using log4net;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Connectivity;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class OctaneTaskManager
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
			Log.Debug("OctaneTaskManager created...");
		}

		public void ShutDown()
		{
			_pollTasksCancellationToken.Cancel();
			Log.Debug("OctaneTaskManager shuted down");
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
			while (!token.IsCancellationRequested)
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
						PluginManager.GetInstance().RestartPlugin();
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
							HandleTasks(taskDef);
						});
					}
				}
			}
			Log.Debug("Task polling - finished");
		}


		private void HandleTasks(string taskData)
		{
			Log.Info($"Received tasks : {taskData}");
			try
			{
				//process task
				var octaneTasks = JsonHelper.DeserializeObject<List<OctaneTask>>(taskData);

				if (octaneTasks == null || octaneTasks.Count == 0)
				{
					Log.Error("Received data does not contains any valid task.");
					return;
				}

				foreach (var octaneTask in octaneTasks)
				{
					var start = DateTime.Now;
					var taskOutput = ExecuteTask(octaneTask?.ResultUrl);
					var end = DateTime.Now;
					Log.Debug($"Task executed in {(long)((end - start).TotalMilliseconds)} ms");

					//prepare response
					int status = HttpMethodEnum.POST.Equals(octaneTask.Method) ? 201 : 200;
					var taskResult = new OctaneTaskResult(status, octaneTask.Id, taskOutput);
					Log.Debug($"Sending result to octane :  { taskResult.Body}");
					_octaneApis.SendTaskResult(taskResult.Id.ToString(), taskResult);
				}
			}
			catch (InvalidCredentialException ex)//if credentials in tfs has been changed
			{
				Log.Error("Failed to process task : " + ex.Message);
				PluginManager.GetInstance().RestartPlugin();
			}
			catch (Exception ex)
			{
				Log.Error("Failed to process task " + ex.Message, ex);
			}
		}

		public string ExecuteTask(Uri taskUrl)
		{
			var taskType = ParseTaskUriToTaskType(taskUrl);
			string result;
			switch (taskType)
			{
				case TaskType.GetJobsList:
					result = JsonHelper.SerializeObject(_tfsApis.GetJobsList());
					break;
				case TaskType.GetJobDetail:
					var jobId = taskUrl.Segments[taskUrl.Segments.Length - 1];
					result = JsonHelper.SerializeObject(_tfsApis.GetJobDetail(jobId));
					break;
				case TaskType.ExecutePipelineRunRequest:
					var joinedProjectName = taskUrl.Segments[taskUrl.Segments.Length - 2].Trim('/');
					var buildParts = joinedProjectName.Split('.');
					_tfsApis.QueueNewBuild(buildParts[0], buildParts[1], buildParts[2]);
					result = "";
					break;
				case TaskType.Undefined:
					Log.Debug($"Undefined task : {taskUrl}");
					result = "";
					break;
				default:
					result = "";
					break;
			}

			return result;
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
