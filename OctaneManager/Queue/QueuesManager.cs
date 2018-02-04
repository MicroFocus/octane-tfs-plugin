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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Scm;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.TestResults;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using MicroFocus.Ci.Tfs.Octane;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Queue
{
	public class QueuesManager
	{

		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private OctaneApis _octaneApis;
		private TfsApis _tfsApis;

		private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
		private Task _generalEventsThread, _scmEventsThread, _testResultsThread;
		private EventList _generalEventsQueue;
		private EventsQueue _scmEventsQueue, _testResultsQueue;


		public QueuesManager(TfsApis tfsApis, OctaneApis octaneApis)
		{
			_octaneApis = octaneApis;
			_tfsApis = tfsApis;
		}

		public TfsApis TfsApis
		{
			get { return _tfsApis; }
		}

		public void Start()
		{
			_generalEventsQueue = PluginManager.GetInstance().GeneralEventsQueue;
			_scmEventsQueue = PluginManager.GetInstance().ScmEventsQueue;
			_testResultsQueue = PluginManager.GetInstance().TestResultsQueue;

			_generalEventsThread = Task.Factory.StartNew(() => ProcessGeneralEvents(_cancellationToken.Token), TaskCreationOptions.LongRunning);
			_scmEventsThread = Task.Factory.StartNew(() => ProcessFinishEvents(_cancellationToken.Token, _scmEventsQueue, "ScmEvents", SendScmEvent, SleepTimeWrapper.Create()), TaskCreationOptions.LongRunning);
			_testResultsThread = Task.Factory.StartNew(() => ProcessFinishEvents(_cancellationToken.Token, _testResultsQueue, "TestResults", SendTestResults, SleepTimeWrapper.Create()), TaskCreationOptions.LongRunning);
			Log.Debug("QueuesManager - started");
		}

		private static void ProcessFinishEvents(CancellationToken token, EventsQueue queue, string queueName, Action<CiEvent> action, SleepTimeWrapper sleepTimeWrapper)
		{
			Log.Debug($"{queueName} task - started");
			while (!token.IsCancellationRequested)
			{
				try
				{
					while (!queue.IsEmpty())
					{
						var ciEvent = queue.Peek();
						action(ciEvent);

						//remove item from _finishedEventsQueue
						queue.Dequeue();
						sleepTimeWrapper.SleepTime = SleepTimeWrapper.DEFAULT_SLEEP_TIME;
					}
				}
				catch (Exception e)
				{
					ExceptionHelper.HandleExceptionAndRestartIfRequired(e, Log, "ProcessFinishEvents");

					sleepTimeWrapper.SleepTime = sleepTimeWrapper.SleepTime * 2;
					if (sleepTimeWrapper.SleepTime > SleepTimeWrapper.MAX_SLEEP_TIME)
					{
						CiEvent ciEvent = queue.Dequeue();
						Log.Error($"Build {ciEvent.BuildInfo} - Impossible to handle event in {queueName} queue. Event is removed from queue.");
						sleepTimeWrapper.SleepTime = SleepTimeWrapper.DEFAULT_SLEEP_TIME;
					}
				}

				Thread.Sleep(sleepTimeWrapper.SleepTime * 1000);//wait before next loop
			}
			Log.Debug($"{queueName} task - finished");
		}

		private void ProcessGeneralEvents(CancellationToken token)
		{
			Log.Debug("GeneralEvent task - started");
			SleepTimeWrapper sleepTimeWrapper = SleepTimeWrapper.Create();
			while (!token.IsCancellationRequested)
			{
				try
				{
					if (!_generalEventsQueue.IsEmpty())
					{
						IList<CiEvent> snapshot = _generalEventsQueue.GetSnapshot();
						_octaneApis.SendEvents(snapshot);

						//post-send treating
						foreach (CiEvent ciEvent in snapshot)
						{
							//1.Log
							Log.Debug($"Build {ciEvent.BuildInfo} - {ciEvent.EventType.ToString().ToUpper()} event is sent");

							//2.Clear original list
							_generalEventsQueue.Remove(ciEvent);
						}
					}
					sleepTimeWrapper.SleepTime = SleepTimeWrapper.DEFAULT_SLEEP_TIME;
				}
				catch (Exception e)
				{
					ExceptionHelper.HandleExceptionAndRestartIfRequired(e, Log, "ProcessGeneralEvents");

					sleepTimeWrapper.SleepTime = sleepTimeWrapper.SleepTime * 2;
					if (sleepTimeWrapper.SleepTime > SleepTimeWrapper.MAX_SLEEP_TIME)
					{
						_generalEventsQueue.Clear();
						Log.Error($"Impossible to handle general events. Event queue is cleared.");
						sleepTimeWrapper.SleepTime = SleepTimeWrapper.DEFAULT_SLEEP_TIME;
					}
				}

				Thread.Sleep(sleepTimeWrapper.SleepTime * 1000);//wait before next loop
			}
			Log.Debug("GeneralEvents task - finished");
		}

		public void ShutDown()
		{
			if (!_cancellationToken.IsCancellationRequested)
			{
				_cancellationToken.Cancel();
				Log.Debug("QueuesManager - stopped");
			}
		}

		public void WaitShutdown()
		{
			_generalEventsThread.Wait();
			_testResultsThread.Wait();
			_scmEventsThread.Wait();
		}

		private void SendScmEvent(CiEvent ciEvent)
		{
			DateTime start = DateTime.Now;
			var scmData = ScmEventHelper.GetScmData(_tfsApis, ciEvent.BuildInfo);
			DateTime end = DateTime.Now;

			int commitCount = 0;
			if (scmData != null)
			{
				commitCount = scmData.Commits.Count;
				var scmEvent = CreateScmEvent(ciEvent, scmData);
				_generalEventsQueue.Add(scmEvent);
			}
			Log.Info($"Build {ciEvent.BuildInfo} - SCM data contains {commitCount} commits. Handling time is {(long)((end - start).TotalMilliseconds)} ms.");
		}

		private static CiEvent CreateScmEvent(CiEvent finishEvent, ScmData scmData)
		{
			var scmEventEvent = finishEvent.Clone();
			scmEventEvent.EventType = CiEventType.Scm;
			scmEventEvent.ScmData = scmData;
			return scmEventEvent;
		}

		private void SendTestResults(CiEvent ciEvent)
		{
			try
			{
				DateTime start = DateTime.Now;
				string msg = "";
				if (_octaneApis.IsTestResultRelevant(ciEvent.Project))
				{
					var run = _tfsApis.GetRunForBuild(ciEvent.BuildInfo.CollectionName, ciEvent.BuildInfo.Project, ciEvent.BuildInfo.BuildId);
					if (run == null)
					{
						msg = "Run was not created for build. No test results.";
					}
					else
					{
						var testResults = _tfsApis.GetTestResultsForRun(ciEvent.BuildInfo.CollectionName, ciEvent.BuildInfo.Project, run.Id.ToString());
						OctaneTestResult octaneTestResult = OctaneTestResutsUtils.ConvertToOctaneTestResult(_octaneApis.PluginInstanceId, ciEvent.Project, ciEvent.BuildId, testResults, run.WebAccessUrl);
						_octaneApis.SendTestResults(octaneTestResult);
						msg = $"TestResults are sent ({octaneTestResult.TestRuns.Count} tests)";
					}
				}
				else
				{
					msg = "GetTestResultRelevant = false";
				}

				DateTime end = DateTime.Now;
				Log.Info($"Build {ciEvent.BuildInfo} - {msg}. Handling time is {(long)((end - start).TotalMilliseconds)} ms.");
			}
			catch (Exception ex)
			{
				Log.Error($"Build {ciEvent.BuildInfo} : error in SendTestResults : {ex.Message}", ex);
				throw ex;
			}
		}
	}

	public class SleepTimeWrapper
	{
		public static readonly int DEFAULT_SLEEP_TIME = 2; //2 seconds
		public static readonly int MAX_SLEEP_TIME = 120; //120 seconds

		public int SleepTime { get; set; } = DEFAULT_SLEEP_TIME;

		public static SleepTimeWrapper Create()
		{
			return new SleepTimeWrapper();
		}
	}
}
