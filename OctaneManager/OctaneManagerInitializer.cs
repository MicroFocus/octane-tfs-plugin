using log4net;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using MicroFocus.Ci.Tfs.Octane.Tools;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class OctaneManagerInitializer : IDisposable
	{
		private static readonly TimeSpan[] _initTimeoutArr = new TimeSpan[] { new TimeSpan(0, 0, 0, 30), new TimeSpan(0, 0, 2, 0), new TimeSpan(0, 0, 10, 0) };
		private int _initFailCounter = 0;
		private OctaneManager _octaneManager = null;
		private Task _octaneInitializationThread = null;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static ConnectionDetails _connectionDetails;

		private static OctaneManagerInitializer instance = new OctaneManagerInitializer();

		private OctaneManagerInitializer()
		{
			ConfigurationManager.ConfigurationChanged += OnConfigurationChanged;
			ReadConfigurationFile();
			StartRestServer();
		}

		private void ReadConfigurationFile()
		{
			try
			{
				_connectionDetails = ConfigurationManager.Read();
			}
			catch (Exception e)
			{
				Log.Error($"Failed to load configuration file : {e.Message}");
				_connectionDetails = null;
			}
		}

		public static OctaneManagerInitializer GetInstance()
		{
			return instance;
		}

		public PluginRunMode RunMode { get; set; } = PluginRunMode.ConsoleApp;

		public void Shutdown()
		{
			Log.Info("OctaneManagerInitializer Shutdown");
			ConfigurationManager.ConfigurationChanged -= OnConfigurationChanged;
			StopRestServer();
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
			Log.Info("StartPlugin");
			if (_connectionDetails == null)
			{
				Log.Error("Cannot StartPlugin as configuration file is not loaded");
				return;
			}

			if (_octaneInitializationThread == null)
			{
				_cancellationTokenSource = new CancellationTokenSource();
				_octaneInitializationThread =
					Task.Factory.StartNew(() => InitializeOctaneManager(_cancellationTokenSource.Token),
						TaskCreationOptions.LongRunning);
			}
		}

		public OctaneManager OctaneManager => _octaneManager;

		private void StartRestServer()
		{
			try
			{
				RestServer.Server.GetInstance().Start();
			}
			catch (Exception e)
			{
				Log.Error($"Failed to start rest server : {e.Message}", e);
			}
		}

		private void StopRestServer()
		{
			try
			{
				RestServer.Server.GetInstance().Stop();
			}
			catch (Exception e)
			{
				Log.Error($"Failed to stop rest server : {e.Message}", e);
			}
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
					_octaneManager = new OctaneManager(RunMode, _connectionDetails);
					_octaneManager.Init();
					_initFailCounter = 0;
				}
				catch (Exception ex)
				{
					Log.Error($"Error initializing octane plugin : {ex.Message}", ex);
					_octaneManager.ShutDown();
					_octaneManager = null;
				}


				//Sleep till next retry
				if (!IsOctaneInitialized())
				{

					int initTimeoutIndex = Math.Min(((int)_initFailCounter / 3), _initTimeoutArr.Length - 1);
					TimeSpan initTimeout = _initTimeoutArr[initTimeoutIndex];
					Log.Info($"Wait {initTimeout.TotalSeconds} secs for next trial of initialization");
					Thread.Sleep(initTimeout);
					_initFailCounter++;
				}
			}
		}

		public bool IsOctaneInitialized()
		{
			return _octaneManager != null && _octaneManager.IsInitialized;
		}

		private void OnConfigurationChanged(object sender, EventArgs e)
		{
			ReadConfigurationFile();
			RestartPlugin();
		}

		public void RestartPlugin()
		{
			StopPlugin();
			StartPlugin();
		}

		public void Dispose()
		{
			Shutdown();
		}
	}
}
