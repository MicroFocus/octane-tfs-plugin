﻿using log4net;
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
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class TfsEventManager
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private OctaneApis _octaneApis;
		private TfsApis _tfsApis;

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
			Log.Debug("TfsEventManager - started");
		}

		public void ShutDown()
		{

			RestBase.BuildEvent -= HandleFinishEventFromWebHook;
			Log.Debug("TfsEventManager - stopped");
		}

		private void HandleFinishEventFromWebHook(object sender, CiEvent finishEvent)
		{
			var startEvent = finishEvent.Clone();
			startEvent.EventType = CiEventType.Started;

			ReportEventAsync(startEvent).GetAwaiter().OnCompleted(() =>
			{
				ReportEventAsync(finishEvent);

			});
		}

		private CiEvent CreateScmEvent(CiEvent finishEvent, ScmData scmData)
		{
			var scmEventEvent = finishEvent.Clone();
			scmEventEvent.EventType = CiEventType.Scm;
			scmEventEvent.ScmData = scmData;
			return scmEventEvent;
		}

		public Task ReportEventAsync(CiEvent ciEvent)
		{
			Task task = Task.Factory.StartNew(() =>
			{
				ReportEvent(ciEvent);
			});
			return task;
		}

		private void ReportEvent(CiEvent ciEvent)
		{
			Log.Debug($"Build {ciEvent.BuildInfo} - handling {ciEvent.EventType.ToString().ToUpper()} event");
			try
			{
				var list = new List<CiEvent>();
				list.Add(ciEvent);

				bool isFinishEvent = ciEvent.EventType.Equals(CiEventType.Finished);
				if (isFinishEvent)
				{
					var scmData = ScmEventHelper.GetScmData(_tfsApis, ciEvent.BuildInfo);
					if (scmData != null)
					{
						list.Add(CreateScmEvent(ciEvent, scmData));
						Log.Debug($"Build {ciEvent.BuildInfo} - scm data contains {scmData.Commits.Count} commits");
					}
					else
					{
						Log.Debug($"Build {ciEvent.BuildInfo} - scm data is empty");
					}
				}

				_octaneApis.SendEvents(list);
				Log.Debug($"Build {ciEvent.BuildInfo} - {list.Count} events succesfully sent");
				if (isFinishEvent)
				{
					SendTestResults(ciEvent.BuildInfo, ciEvent.Project, ciEvent.BuildId);
				}
			}
			catch (InvalidCredentialException e)
			{
				Log.Error($"ReportEvent failed with TFS : {e.Message}");
				PluginManager.GetInstance().RestartPlugin();
			}
			catch (Exception e)
			{
				Log.Error($"ReportEvent failed : {e.Message}", e);
			}
		}

		public void SendTestResults(TfsBuildInfo buildInfo, string projectCiId, string buildCiId)
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
