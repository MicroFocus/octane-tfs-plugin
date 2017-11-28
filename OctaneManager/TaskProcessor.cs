using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Hpe.Nga.Api.Core.Connector;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using MicroFocus.Ci.Tfs.Octane.Dto.Connectivity;
using MicroFocus.Ci.Tfs.Octane.Tools;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane
{
    internal class TaskProcessor
    {
        private readonly TfsManager _tfsManager;
        private enum TaskType
        {
            GetJobsList,
            GetJobDetail,
            Undefined

        }
        public TaskProcessor(TfsManager tfsManager)
        {
            _tfsManager = tfsManager;
        }

        public string ProcessTask(Uri taskUrl)
        {
            switch (ParseUriPath(taskUrl))
            {
                case TaskType.GetJobsList:
                    return JsonConvert.SerializeObject(_tfsManager.GetJobsList());
                case TaskType.GetJobDetail:
                    var jobId = taskUrl.Segments[taskUrl.Segments.Length - 1];
                    return JsonConvert.SerializeObject(_tfsManager.GetJobDetail(jobId));
                case TaskType.Undefined:
                    Trace.WriteLine($"Undefined task : {taskUrl}");
                    return null;
                default:
                    return null;

            }            
        }

        private static TaskType ParseUriPath(Uri uriPath)
        {
            var uri = uriPath;
            var uriPathList = uriPath.Segments.ToList();
            for (var i = 0; i < uriPathList.Count; i++)
            {
                uriPathList[i] = uriPathList[i].Replace("/", "");
            }

            var octaneParams = uriPathList.GetRange(uriPathList.IndexOf("v1")+1,
                uriPathList.Count - uriPathList.IndexOf("v1") - 1);

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
            }

            Trace.WriteLine($"Found undefined taskUrl type `{param}` in uri: {uriPath}");

            return TaskType.Undefined;

        }



    }
}
