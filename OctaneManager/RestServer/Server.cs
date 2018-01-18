/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using log4net;
using Nancy;
using Nancy.Hosting.Self;
using System;
using System.Reflection;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.RestServer
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
		        Log.Debug("RestServer is already running");
                return;		       
		    }

			var hostConfigs = new HostConfiguration { UrlReservations = { CreateAutomatically = true } };
			var serverUri = new Uri($"http://localhost:{_port}");
			var myServer = new NancyHost(serverUri, new DefaultNancyBootstrapper(), hostConfigs);

			myServer.Start();
			Log.Info($"RestServer - started on {serverUri}");
			_server = myServer;
		}

		public void Stop()
		{
			if (IsStarted())
			{
				_server.Stop();
				_server = null;
			}
			Log.Info("RestServer - stopped");
		}

		private bool IsStarted()
		{
			return _server != null;
		}
	}
}
