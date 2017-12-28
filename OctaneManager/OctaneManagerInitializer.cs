using log4net;
using MicroFocus.Ci.Tfs.Octane.Tools;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class OctaneManagerInitializer : IDisposable
	{
		private static readonly TimeSpan _initTimeout = new TimeSpan(0, 0, 0, 10);
		private OctaneManager _octaneManager = null;
		private Task _octaneInitializationThread = null;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static OctaneManagerInitializer instance = new OctaneManagerInitializer();

		private OctaneManagerInitializer()
		{

		}

		public static OctaneManagerInitializer GetInstance()
		{
			return instance;
		}

		public PluginRunMode RunMode { get; set; } = PluginRunMode.ConsoleApp;

		public void Start()
		{
			Log.Info("OctaneManagerInitializer start");
			StartServer();
			StartPlugin();
		}

		public void Stop()
		{
			Log.Info("OctaneManagerInitializer stop");
			StopServer();
			StopPlugin();
		}


		public void StopPlugin()
        {
			if (_octaneManager != null)
			{
				_cancellationTokenSource.Cancel();
				_octaneManager.ShutDown();
			}

            _octaneManager = null;
			_octaneInitializationThread = null;
        }

        public void StartPlugin()
        {
			if (_octaneInitializationThread == null)
			{
				_cancellationTokenSource = new CancellationTokenSource();
				_octaneInitializationThread =
					Task.Factory.StartNew(() => InitializeOctaneManager(_cancellationTokenSource.Token),
						TaskCreationOptions.LongRunning);
			}
		}

        public OctaneManager OctaneManager => _octaneManager;

		public void StartServer()
		{
			RestServer.Server.GetInstance().Start();
		}

		public void StopServer()
		{
			RestServer.Server.GetInstance().Stop();
		}	  

		private void InitializeOctaneManager(CancellationToken token)
		{
			while (!IsOctaneInitialized())
			{
				if (token.IsCancellationRequested)
				{
					Log.Info("Octane initialization thread was requested to quit!");
					break;
				}
				try
				{
					if (_octaneManager == null)
					{
						_octaneManager = new OctaneManager(RunMode);
					}

					_octaneManager.Init();

				}
				catch (Exception ex)
				{
					Log.Error($"Error initializing octane plugin : {ex.Message}", ex);
				}

				//Sleep till next retry
				if (!IsOctaneInitialized())
				{
					Log.Info($"Wait {_initTimeout.TotalSeconds} secs for next trial of initialization");
				}
				
				Thread.Sleep(_initTimeout);

			}
		}

		public bool IsOctaneInitialized()
		{
			return _octaneManager != null && _octaneManager.IsInitialized;
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
