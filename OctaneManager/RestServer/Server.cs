using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Nancy;
using Nancy.Hosting.Self;

namespace MicroFocus.Ci.Tfs.Octane.RestServer
{
    public class Server
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _port;
        private NancyHost _server;
        public Server(int port = 4567)
        {
            _port = port;
        }

        public void Start()
        {
            var hostConfigs = new HostConfiguration { UrlReservations = { CreateAutomatically = true } };
            var serverUri = new Uri($"http://localhost:{_port}");
            var host = new NancyHost(serverUri, new DefaultNancyBootstrapper(), hostConfigs);

            host.Start();
            Log.Info($"Running on {serverUri}");
            _server = host;
        }

        public void Stop()
        {
            _server.Stop();
            Log.Info("Server stopped.");
        }
    }
}
