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
using MicroFocus.Ci.Tfs.Octane.Tools;

namespace MicroFocus.Ci.Tfs.Octane
{
    internal class TaskProcessor
    {
        private enum TaskType
        {
            GetJobsList,
            Undefined

        }
        public TaskProcessor()
        {

        }

        public string ProcessTask(string task)
        {
            switch (ParseUriPath(task))
            {
                case TaskType.GetJobsList:


            }
        }

        private TaskType ParseUriPath(string uriPath)
        {
            var uri = new Uri(uriPath);

            var param = uri.Segments[uri.Segments.Length - 1];
            if (param == "jobs" && HttpUtility.ParseQueryString(uri.Query)["parameters"] == "false")
            {
                return TaskType.GetJobsList;
            }

            Trace.WriteLine($"Found undefined task type `{param}` in uri: {uriPath}");

            return TaskType.Undefined;

        }



    }
}
