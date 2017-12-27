using log4net;
using Nancy;
using Nancy.Hosting.Self;
using System;
using System.Reflection;

namespace MicroFocus.Ci.Tfs.Octane.RestServer
{
	public class Server : IDisposable
	{
		private int _port = 4567;
		private NancyHost _server;

		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static Server instance = new Server();

		private Server()
		{

		}

		public static Server GetInstance()
		{
			return instance;
		}

		public int Port
		{
			get
			{
				return _port;
			}
			set
			{
				_port = value;
				if (IsStarted())
				{
					Stop();
					Start();
				}
			}
		}

		public void Dispose()
		{
			Stop();
		}

		public void Start()
		{
		    if (IsStarted())
		    {
		        Log.Debug("Server is already running...");
                return;		       
		    }

			var hostConfigs = new HostConfiguration { UrlReservations = { CreateAutomatically = true } };
			var serverUri = new Uri($"http://localhost:{_port}");
			var host = new NancyHost(serverUri, new DefaultNancyBootstrapper(), hostConfigs);

			host.Start();
			Log.Info($"Running on {serverUri}");
			_server = host;
		}

		public void Stop()
		{
			if (IsStarted())
			{
				_server.Stop();
				_server = null;
				Log.Info("Server stopped.");
			}
		}

		private bool IsStarted()
		{
			return _server != null;
		}
	}
}
