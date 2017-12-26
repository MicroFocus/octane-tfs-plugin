﻿using log4net;
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
		private PluginRunMode _runMode;

		public void Start(PluginRunMode runMode)
		{
			if (_cancellationTokenSource.IsCancellationRequested)
			{
				throw new InvalidOperationException("You cannot use the same initializer after it stopped");
			}

			this._runMode = runMode;
			Log.Info("OctaneManagerInitializer start");

			Octane.RestServer.Server.GetInstance().Start();
			if (_octaneInitializationThread == null)
			{
				_octaneInitializationThread =
					Task.Factory.StartNew(() => InitializeOctaneManager(_cancellationTokenSource.Token),
						TaskCreationOptions.LongRunning);
			}
		}

		public OctaneManager OctaneManager
		{
			get
			{
				return _octaneManager;
			}
		}

		public void ShutDown()
		{
			_cancellationTokenSource.Cancel();
			_octaneManager.ShutDown();
			RestServer.Server.GetInstance().Stop();
		}

		public void WaitShutDown()
		{
			if (_octaneManager != null)
			{
				_octaneManager.WaitShutdown();
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
					if (_octaneManager == null)
					{
						_octaneManager = new OctaneManager(_runMode);
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
			ShutDown();
		}
	}
}
