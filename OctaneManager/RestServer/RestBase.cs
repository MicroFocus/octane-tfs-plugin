using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace MicroFocus.Ci.Tfs.Octane.RestServer
{
    public class RestBase : NancyModule
    {
        public RestBase()
        {
            Post["/"] = _ => "Received POST request";
            Get["/"] = _ => "Hello 2";
            Get["/products/{id}"] = _ =>
            {
                return "Hello";
            };
        }
    }
}
