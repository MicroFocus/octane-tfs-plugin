using log4net;
using MicroFocus.Adm.Octane.Api.Core.Connector.Exceptions;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Scm;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.TestResults;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Queue;
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
		private const int DEFAULT_SLEEP_TIME = 2 * 1000; //2 seconds
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private OctaneApis _octaneApis;
		private TfsApis _tfsApis;

		private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
		private Task _generalEventsThread;
		private Task _finishEventsThread;
		GeneralEventsQueue _generalEventsQueue;
		FinishedEventsQueue _finishedEventsQueue;

		public TfsEventManager(TfsApis tfsApis, OctaneApis octaneApis)
		{
			_octaneApis = octaneApis;
			_tfsApis = tfsApis;
		}

		public void Start()
		{
			_generalEventsQueue = PluginManager.GetInstance().GeneralEventsQueue;
			_finishedEventsQueue = PluginManager.GetInstance().FinishedEventsQueue;

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
					while (!_finishedEventsQueue.IsEmpty())
					{
						var ciEvent = _finishedEventsQueue.Peek();
						//handle scm event
						var scmData = ScmEventHelper.GetScmData(_tfsApis, ciEvent.BuildInfo);
						if (scmData != null)
						{
							Log.Debug($"Build {ciEvent.BuildInfo} - scm data contains {scmData.Commits.Count} commits");
							var scmEvent = CreateScmEvent(ciEvent, scmData);
							_generalEventsQueue.Add(scmEvent);
						}
						else
						{
							Log.Debug($"Build {ciEvent.BuildInfo} - scm data is empty");
						}

						//handle test result
						SendTestResults(ciEvent.BuildInfo, ciEvent.Project, ciEvent.BuildId);

						//remove item from _finishedEventsQueue
						_finishedEventsQueue.Dequeue();
					}
				}
				catch (Exception e)
				{
					ExceptionHelper.HandleExceptionAndRestartIfRequired(e, Log, "ProcessFinishEvents");
				}

				Thread.Sleep(DEFAULT_SLEEP_TIME);//wait before next loop
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
					if (!_generalEventsQueue.IsEmpty())
					{
						IList<CiEvent> snapshot = _generalEventsQueue.GetSnapshot();
						_octaneApis.SendEvents(snapshot);

						//post-send treating
						foreach (CiEvent ciEvent in snapshot)
						{
							//1.Log
							Log.Debug($"Build {ciEvent.BuildInfo} - {ciEvent.EventType.ToString().ToUpper()} event is sent");


							//2.Add finish events to special list for futher handling : scm event and test result sending
							bool isFinishEvent = ciEvent.EventType.Equals(CiEventType.Finished);
							if (isFinishEvent)
							{
								_finishedEventsQueue.Add(ciEvent);
							}

							//3.Clear original list
							_generalEventsQueue.Remove(ciEvent);
						}
					}
				}
				catch (Exception e)
				{
					ExceptionHelper.HandleExceptionAndRestartIfRequired(e, Log, "ProcessGeneralEvents");
				}

				Thread.Sleep(DEFAULT_SLEEP_TIME);//wait before next loop
			}
			Log.Debug("GeneralEvents task - finished");
		}


		public void ShutDown()
		{
			if (!_cancellationToken.IsCancellationRequested)
			{
				_cancellationToken.Cancel();
				Log.Debug("TfsEventManager - stopped");
			}
		}

		public void WaitShutdown()
		{
			_generalEventsThread.Wait();
			_finishEventsThread.Wait();
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
