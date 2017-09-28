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
        private readonly TfsManager _tfsManager = new TfsManager();
        private enum TaskType
        {
            GetJobsList,
            Undefined

        }
        public TaskProcessor()
        {

        }

        public string ProcessTask(Uri taskUrl)
        {
            switch (ParseUriPath(taskUrl))
            {
                case TaskType.GetJobsList:
                    return JsonConvert.SerializeObject(_tfsManager.GetJobsList());

                case TaskType.Undefined:
                    return null;
                default:
                    return null;

            }            
        }

        private static TaskType ParseUriPath(Uri uriPath)
        {
            var uri = uriPath;

            var param = uri.Segments[uri.Segments.Length - 1];
            if (param == "jobs" && HttpUtility.ParseQueryString(uri.Query)["parameters"] == "true")
            {
                return TaskType.GetJobsList;
            }

            Trace.WriteLine($"Found undefined taskUrl type `{param}` in uri: {uriPath}");

            return TaskType.Undefined;

        }



    }
}
