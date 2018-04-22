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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.RestServer;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Queue;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class PluginManager : IDisposable
	{
		private static readonly int[] _initTimeoutInMinutesArr = new[] { 1, 3, 10 };
		private int _initFailCounter = 0;

		private Task _octaneInitializationThread = null;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static ConnectionDetails _connectionDetails;

		private readonly EventList _generalEventsQueue = new EventList();
		private readonly EventsQueue _testResultsQueue = new EventsQueue();
		private readonly EventsQueue _scmEventsQueue = new EventsQueue();

		private QueuesManager _queuesManager;
		private OctaneTaskManager _taskManager;

		private StatusEnum _pluginStatus = StatusEnum.Stopped;

		private static PluginManager instance = new PluginManager();

		public enum StatusEnum { Stopped, Connected, Connecting, Stopping }


		private PluginManager()
		{
			Thread.Sleep(5000);
			ConfigurationManager.ConfigurationChanged += OnConfigurationChanged;
			ReadConfigurationFile();
			StartRestServer();
		}

		public EventList GeneralEventsQueue => _generalEventsQueue;

		public EventsQueue TestResultsQueue => _testResultsQueue;

		public EventsQueue ScmEventsQueue => _scmEventsQueue;

		public StatusEnum Status
		{
			get { return _pluginStatus; }

			internal set
			{
				_pluginStatus = value;
				Log.Info($"Plugin status set to : {_pluginStatus.ToString()}");
			}
		}

		private void ReadConfigurationFile()
		{
			try
			{
				ConnectionDetails tempConf = ConfigurationManager.Read(true);
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

			if (_queuesManager != null)
			{
				_queuesManager.ShutDown();
			}

			if (_taskManager != null)
			{
				_taskManager.ShutDown();
			}
		}

		public void StopPlugin(bool waitShutdown)
		{
			Status = StatusEnum.Stopping;
			StopAllThreads();
			if (waitShutdown)
			{
				if (_octaneInitializationThread != null)
				{
					_octaneInitializationThread.Wait();
				}

				if (_queuesManager != null)
				{
					_queuesManager.WaitShutdown();
				}

				if (_taskManager != null)
				{
					_taskManager.WaitShutdown();
				}
			}

			_octaneInitializationThread = null;
			_taskManager = null;
			_queuesManager = null;
			Status = StatusEnum.Stopped;
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
			Status = StatusEnum.Connecting;
			while (Status != StatusEnum.Connected && !token.IsCancellationRequested)
			{
				try
				{
					TfsApis tfsApis = ConnectionCreator.CreateTfsConnection(_connectionDetails);
					OctaneApis octaneApis = ConnectionCreator.CreateOctaneConnection(_connectionDetails);

					_taskManager = new OctaneTaskManager(tfsApis, octaneApis);
					_taskManager.Start();
					_queuesManager = new QueuesManager(tfsApis, octaneApis);
					_queuesManager.Start();

					_initFailCounter = 0;
					Status = StatusEnum.Connected;
				}
				catch (Exception ex)
				{
					Log.Error($"Error in StartPlugin : {ex.Message}", ex);
					if (_queuesManager != null)
					{
						_queuesManager.ShutDown();
						_queuesManager = null;
					}
					if (_taskManager != null)
					{
						_taskManager.ShutDown();
						_taskManager = null;
					}
				}

				//Sleep till next retry
				if (Status != StatusEnum.Connected)
				{
					int initTimeoutIndex = Math.Min((_initFailCounter / 3), _initTimeoutInMinutesArr.Length - 1);
					int initTimeoutMinutes = _initTimeoutInMinutesArr[initTimeoutIndex];
					Log.Info($"Wait {initTimeoutMinutes} minute(s) till next initialization attempt...");
					Thread.Sleep(initTimeoutMinutes * 1000 * 60);
					_initFailCounter++;
				}
			}
		}

		private void OnConfigurationChanged(object sender, EventArgs e)
		{
			ReadConfigurationFile();
			RestartPlugin();
		}

		public void RestartPlugin()
		{
			Log.Info($"Plugin is restarting");

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

		public void HandleFinishEvent(CiEvent ciEvent)
		{
			Task.Factory.StartNew(() =>
			{
				try
				{
					if (_queuesManager != null)
					{
						_queuesManager.TfsApis.ConnectionValidation("warm-up");
					}
				}
				finally
				{
					_scmEventsQueue.Add(ciEvent);
					_testResultsQueue.Add(ciEvent);

					Thread.Sleep(5000);
					_generalEventsQueue.Add(ciEvent);
				}


			});
		}
	}
}
