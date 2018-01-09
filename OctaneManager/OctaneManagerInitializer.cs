using log4net;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.RestServer;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class OctaneManagerInitializer : IDisposable
	{
		private static readonly TimeSpan[] _initTimeoutArr = new TimeSpan[] { new TimeSpan(0, 0, 0, 30), new TimeSpan(0, 0, 2, 0), new TimeSpan(0, 0, 10, 0) };
		private int _initFailCounter = 0;
		private TfsEventManager _octaneManager = null;
		private Task _octaneInitializationThread = null;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static ConnectionDetails _connectionDetails;

		OctaneTaskManager _octaneTaskManager;

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

		public void Shutdown()
		{
			Log.Info("OctaneManagerInitializer Shutdown");
			ConfigurationManager.ConfigurationChanged -= OnConfigurationChanged;
			StopRestServer();
			StopPlugin();
		}

		public void StopPlugin()
		{
			_cancellationTokenSource.Cancel();
			_octaneInitializationThread = null;

			if (_octaneManager != null)
			{
				_octaneManager.ShutDown();
				_octaneManager = null;
			}

			if (_octaneTaskManager != null)
			{
				_octaneTaskManager.ShutDown();
				_octaneTaskManager = null;
			}
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

		public TfsEventManager OctaneManager => _octaneManager;

		private void StartRestServer()
		{
			try
			{
				Server.GetInstance().Start();
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
				Server.GetInstance().Stop();
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
					TfsApis tfsApis = ConnectionCreator.CreateTfsConnection(_connectionDetails);
					OctaneApis octaneApis = ConnectionCreator.CreateOctaneConnection(_connectionDetails);

					_octaneTaskManager = new OctaneTaskManager(tfsApis, octaneApis);
					_octaneManager = new TfsEventManager(tfsApis, octaneApis);

					_octaneTaskManager.Start();
					_octaneManager.Init();
					_initFailCounter = 0;
				}
				catch (Exception ex)
				{
					Log.Error($"Error initializing octane plugin : {ex.Message}", ex);
					if (_octaneManager != null)
					{
						_octaneManager.ShutDown();
						_octaneManager = null;
					}
					if (_octaneTaskManager != null)
					{
						_octaneTaskManager.ShutDown();
						_octaneTaskManager = null;
					}

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
