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
            Get["/build-event/"] = _ =>
            {
                return "Hello";
            };
        }
    }
}
