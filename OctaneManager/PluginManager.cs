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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Queue;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class PluginManager : IDisposable
	{
		private static readonly int[] _initTimeoutInMinutesArr = new [] { 1, 3, 10 };
		private int _initFailCounter = 0;

		private Task _octaneInitializationThread = null;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static ConnectionDetails _connectionDetails;

		private GeneralEventsQueue _generalEventsQueue;
		private FinishedEventsQueue _finishedEventsQueue;

		private TfsEventManager _eventManager;
		private OctaneTaskManager _taskManager;

		private static PluginManager instance = new PluginManager();

		private PluginManager()
		{
			Thread.Sleep(5000);
			ConfigurationManager.ConfigurationChanged += OnConfigurationChanged;
			ReadConfigurationFile();
			StartRestServer();
			_generalEventsQueue = new GeneralEventsQueue();
			_finishedEventsQueue = new FinishedEventsQueue();
		}

		public GeneralEventsQueue GeneralEventsQueue => _generalEventsQueue;

		public FinishedEventsQueue FinishedEventsQueue => _finishedEventsQueue;

		private void ReadConfigurationFile()
		{
			try
			{
				ConnectionDetails tempConf = ConfigurationManager.Read();
				ConnectionCreator.CheckMissingValues(tempConf);
				_connectionDetails = tempConf;
			}
			catch (Exception e)
			{
				Log.Error($"Failed to load configuration file : {e.Message}");
				_connectionDetails = null;
			}
		}

		public static PluginManager GetInstance()
		{
			return instance;
		}

		public void Shutdown()
		{
			Log.Info("PluginManager Shutdown");
			ConfigurationManager.ConfigurationChanged -= OnConfigurationChanged;
			StopRestServer();
			StopPlugin(false);
		}


		private void StopAllThreads()
		{
			_cancellationTokenSource.Cancel();

			if (_eventManager != null)
			{
				_eventManager.ShutDown();
			}

			if (_taskManager != null)
			{
				_taskManager.ShutDown();
			}
		}

		public void StopPlugin(bool waitShutdown)
		{
			StopAllThreads();
			if (waitShutdown)
			{
				if (_octaneInitializationThread != null)
				{
					_octaneInitializationThread.Wait();
				}

				if (_eventManager != null)
				{
					_eventManager.WaitShutdown();
				}

				if (_taskManager != null)
				{
					_taskManager.WaitShutdown();
				}
			}

			_octaneInitializationThread = null;
			_taskManager = null;
			_eventManager = null;
		}

		public void StartPlugin()
		{
			Log.Info("StartPlugin");
			if (_connectionDetails == null)
			{
				Log.Error("Cannot StartPlugin as configuration file is not loaded properly");
				return;
			}

			if (_octaneInitializationThread == null)
			{
				_cancellationTokenSource = new CancellationTokenSource();
				_octaneInitializationThread = Task.Factory.StartNew(() => StartPluginInternal(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
			}
		}

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

		private void StartPluginInternal(CancellationToken token)
		{
			while (!IsInitialized() && !token.IsCancellationRequested)
			{
				try
				{
					TfsApis tfsApis = ConnectionCreator.CreateTfsConnection(_connectionDetails);
					OctaneApis octaneApis = ConnectionCreator.CreateOctaneConnection(_connectionDetails);

					_taskManager = new OctaneTaskManager(tfsApis, octaneApis);
					_taskManager.Start();
					_eventManager = new TfsEventManager(tfsApis, octaneApis);
					_eventManager.Start();

					_initFailCounter = 0;
				}
				catch (Exception ex)
				{
					Log.Error($"Error in StartPlugin : {ex.Message}", ex);
					if (_eventManager != null)
					{
						_eventManager.ShutDown();
						_eventManager = null;
					}
					if (_taskManager != null)
					{
						_taskManager.ShutDown();
						_taskManager = null;
					}
				}

				//Sleep till next retry
				if (!IsInitialized())
				{
					int initTimeoutIndex = Math.Min((_initFailCounter / 3), _initTimeoutInMinutesArr.Length - 1);
					int initTimeoutMinutes = _initTimeoutInMinutesArr[initTimeoutIndex];
					Log.Info($"Wait {initTimeoutMinutes} minute(s) for next trial of initialization");
					Thread.Sleep(initTimeoutMinutes*1000*60);
					_initFailCounter++;
				}
			}
		}

		public bool IsInitialized()
		{
			return _eventManager != null && _taskManager != null;
		}

		private void OnConfigurationChanged(object sender, EventArgs e)
		{
			ReadConfigurationFile();
			RestartPlugin();
		}

		public void RestartPlugin()
		{
			Log.Info($"Plugin restarted");

			StopAllThreads();
			Task.Factory.StartNew(() =>
			{
				StopPlugin(true);
				StartPlugin();
			});
		}

		public void Dispose()
		{
			Shutdown();
		}
	}
}
