using Hpe.Nga.Api.Core.Connector.Exceptions;
using log4net;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Scm;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.TestResults;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.RestServer;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class TfsEventManager
	{
		private const int DEFAULT_SLEEP_time = 2 * 1000; //2 seconds
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private OctaneApis _octaneApis;
		private TfsApis _tfsApis;

		private static List<CiEvent> _allEventList = new List<CiEvent>();
		private static Queue<CiEvent> _finishEventQueue = new Queue<CiEvent>();
		private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
		private Task _generalEventsThread;
		private Task _finishEventsThread;

		public TfsEventManager(TfsApis tfsApis, OctaneApis octaneApis)
		{
			_octaneApis = octaneApis;
			_tfsApis = tfsApis;



			Log.Debug("TfsEventManager - created");
		}

		public void Start()
		{
			if (RunModeManager.GetInstance().RunMode == PluginRunMode.ConsoleApp)
			{
				RestBase.BuildEvent += HandleFinishEventFromWebHook;
			}

			_generalEventsThread = Task.Factory.StartNew(() => ProcessGeneralEvents(_cancellationToken.Token), TaskCreationOptions.LongRunning);
			_finishEventsThread = Task.Factory.StartNew(() => ProcessFinishEvents(_cancellationToken.Token), TaskCreationOptions.LongRunning);
			Log.Debug("TfsEventManager - started");
		}

		private void ProcessFinishEvents(CancellationToken token)
		{
			Log.Debug("FinishEvent task - started");
			while (!token.IsCancellationRequested)
			{
				try
				{
					while (_finishEventQueue.Count > 0)
					{
						var ciEvent = _finishEventQueue.Dequeue();
						//handle scm event
						var scmData = ScmEventHelper.GetScmData(_tfsApis, ciEvent.BuildInfo);
						if (scmData != null)
						{
							var scmEvent = CreateScmEvent(ciEvent, scmData);
							AddEvent(scmEvent);
							Log.Debug($"Build {ciEvent.BuildInfo} - scm data contains {scmData.Commits.Count} commits");
						}
						else
						{
							Log.Debug($"Build {ciEvent.BuildInfo} - scm data is empty");
						}


						//send test result
						SendTestResults(ciEvent.BuildInfo, ciEvent.Project, ciEvent.BuildId);
					}
				}
				catch (InvalidCredentialException e)
				{
					Log.Error($"ProcessFinishEvents failed : {e.Message}");
					PluginManager.GetInstance().RestartPlugin();
				}
				catch (Exception e)
				{
					Log.Error($"ProcessFinishEvents failed : {e.Message}", e);
				}


				Thread.Sleep(DEFAULT_SLEEP_time);//wait before next loop
			}
			Log.Debug("FinishEvents task - finished");
		}

		private void ProcessGeneralEvents(CancellationToken token)
		{
			Log.Debug("GeneralEvent task - started");
			while (!token.IsCancellationRequested)
			{
				try
				{
					if (_allEventList.Count > 0)
					{
						List<CiEvent> snapshot = new List<CiEvent>(_allEventList);
						_octaneApis.SendEvents(snapshot);

						//post-send treating
						foreach (CiEvent ciEvent in snapshot)
						{
							//1.Log
							Log.Debug($"Build {ciEvent.BuildInfo} - {ciEvent.EventType.ToString().ToUpper()} event is sent");


							//2.Add finish events to special list for futher handling : scm event and test result sending
							bool isFinishEvent = ciEvent.EventType.Equals(CiEventType.Finished);
							_finishEventQueue.Enqueue(ciEvent);

							//3.Clear original list
							_allEventList.Remove(ciEvent);
						}
					}
				}
				catch (Exception e)
				{
					Log.Error($"ProcessGeneralEvents failed : {e.Message}", e);
					if (e is ServerUnavailableException || e is InvalidCredentialException)
					{
						PluginManager.GetInstance().RestartPlugin();
						continue;
					}
				}

				Thread.Sleep(DEFAULT_SLEEP_time);//wait before next loop
			}
			Log.Debug("GeneralEvents task - finished");
		}

		public void AddEvent(CiEvent ciEvent)
		{
			_allEventList.Add(ciEvent);
		}

		public void ClearQueues()
		{
			_allEventList.Clear();
			_finishEventQueue.Clear();
		}

		public void ShutDown()
		{
			RestBase.BuildEvent -= HandleFinishEventFromWebHook;
			_cancellationToken.Cancel();
			Log.Debug("TfsEventManager - stopped");
		}

		public dynamic GetQueueStatus()
		{
			Dictionary<string, int> map = new Dictionary<string, int>();
			map["allEventList"] = _allEventList.Count;
			map["finishEventQueue"] = _finishEventQueue.Count;
			return map;
		}

		public void WaitShutdown()
		{
			_generalEventsThread.Wait();
			_finishEventsThread.Wait();
		}

		private void HandleFinishEventFromWebHook(object sender, CiEvent finishEvent)
		{
			var startEvent = finishEvent.Clone();
			startEvent.EventType = CiEventType.Started;

			AddEvent(startEvent);
			AddEvent(finishEvent);
		}

		private CiEvent CreateScmEvent(CiEvent finishEvent, ScmData scmData)
		{
			var scmEventEvent = finishEvent.Clone();
			scmEventEvent.EventType = CiEventType.Scm;
			scmEventEvent.ScmData = scmData;
			return scmEventEvent;
		}

		private void SendTestResults(TfsBuildInfo buildInfo, string projectCiId, string buildCiId)
		{
			try
			{
				if (_octaneApis.IsTestResultRelevant(projectCiId))
				{
					var run = _tfsApis.GetRunForBuid(buildInfo.CollectionName, buildInfo.Project, buildInfo.BuildId);
					if (run == null)
					{
						Log.Debug($"Build {buildInfo} - run was not created for build. No test results");
					}
					else
					{
						var testResults = _tfsApis.GetTestResultsForRun(buildInfo.CollectionName, buildInfo.Project, run.Id.ToString());
						OctaneTestResult octaneTestResult = OctaneUtils.ConvertToOctaneTestResult(_octaneApis.PluginInstanceId, projectCiId, buildCiId, testResults, run.WebAccessUrl);
						_octaneApis.SendTestResults(octaneTestResult);

						Log.Debug($"Build {buildInfo} - testResults are sent ({octaneTestResult.TestRuns.Count} tests)");
					}
				}
				else
				{
					Log.Debug($"Build {buildInfo} - GetTestResultRelevant=false for project {projectCiId}");
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Build {buildInfo} : error in SendTestResults : {ex.Message}", ex);
				throw ex;
			}
		}
	}
}
