
using System;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace MicroFocus.Ci.Tfs.Octane.Tools
{
	internal class TaskProcessor
	{
		private readonly TfsManager _tfsManager;
		private enum TaskType
		{
			GetJobsList,
			GetJobDetail,
			ExecutePipelineRunRequest,
			Undefined

		}
		public TaskProcessor(TfsManager tfsManager)
		{
			_tfsManager = tfsManager;
		}

		public string ProcessTask(HttpMethodEnum method, Uri taskUrl)
		{
			var taskType = ParseUriPath(taskUrl);
			string result;
			switch (taskType)
			{
				case TaskType.GetJobsList:
					result= JsonHelper.SerializeObject(_tfsManager.GetJobsList());
					break;
				case TaskType.GetJobDetail:
					var jobId = taskUrl.Segments[taskUrl.Segments.Length - 1];
					result= JsonHelper.SerializeObject(_tfsManager.GetJobDetail(jobId));
					break;
				case TaskType.ExecutePipelineRunRequest:
					var joinedProjectName = taskUrl.Segments[taskUrl.Segments.Length - 2].Trim('/');
					var buildParts = joinedProjectName.Split('.');
					_tfsManager.QueueNewBuild(buildParts[0], buildParts[1], buildParts[2]);
					result = "";
					break;
				case TaskType.Undefined:
					Trace.WriteLine($"Undefined task : {taskUrl}");
					result = "";
					break;
				default:
					result = "";
					break;

			}
			
			return result;
		}

		private static TaskType ParseUriPath(Uri uriPath)
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

			Trace.WriteLine($"Found undefined taskUrl type `{param}` in uri: {uriPath}");

			return TaskType.Undefined;

		}



	}
}
