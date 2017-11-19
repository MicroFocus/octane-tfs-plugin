using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Hosting.Self;

namespace MicroFocus.Ci.Tfs.Octane.RestServer
{
    public class Server
    {
        private readonly int _port;
        private NancyHost _server;
        public Server(int port = 4567)
        {
            _port = port;            
        }

        public void Start()
        {
            var hostConfigs = new HostConfiguration {UrlReservations = {CreateAutomatically = true}};
            var serverUri = new Uri($"http://localhost:{_port}");
            using (var host = new NancyHost(serverUri, new DefaultNancyBootstrapper(), hostConfigs))
            {            
                host.Start();
                Trace.WriteLine($"Running on {serverUri.AbsolutePath}");
                _server = host;
            }

        }

        public void Stop()
        {
            _server.Stop();
        }
    }
}
