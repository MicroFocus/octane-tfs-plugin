
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
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class OctaneManager
	{

		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private OctaneApis _octaneApis;
		private TfsApis _tfsApis;

		public OctaneManager(TfsApis tfsApis, OctaneApis octaneApis)
		{
			_octaneApis = octaneApis;
			_tfsApis = tfsApis;

			if (RunModeManager.GetInstance().RunMode == PluginRunMode.ConsoleApp)
			{
				RestBase.BuildEvent += RestBase_BuildEvent;
			}

			Log.Debug("Octane manager created...");
		}

		public void ShutDown()
		{
			IsInitialized = false;

			RestBase.BuildEvent -= RestBase_BuildEvent;
			Log.Debug("Octane manager shuted down");
		}

		public bool IsInitialized { get; protected set; } = false;


		public void Init()
		{
			IsInitialized = true;
			Log.Debug($"Octane manager initialized successfully");
		}

		private void RestBase_BuildEvent(object sender, CiEvent finishEvent)
		{
			if (IsInitialized)
			{
				CiEvent startEvent = CreateStartEvent(finishEvent);
				ReportEventAsync(startEvent).GetAwaiter().OnCompleted(() =>
				{
					ReportEventAsync(finishEvent);

				});
			}
		}

		private CiEvent CreateStartEvent(CiEvent finishEvent)
		{
			var startEvent = finishEvent.Clone();
			startEvent.EventType = CiEventType.Started;

			return startEvent;
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
			Log.Debug($"{ciEvent.BuildInfo} - handling {ciEvent.EventType.ToString()} event");
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
						Log.Debug($"{ciEvent.BuildInfo} - scm data contains {scmData.Commits.Count} commits");
					}
					else
					{
						Log.Debug($"{ciEvent.BuildInfo} - scm data is empty");
					}
				}


				_octaneApis.SendEvents(list);
				Log.Debug($"{ciEvent.BuildInfo} - {list.Count} events succesfully sent");
				if (isFinishEvent)
				{
					SendTestResults(ciEvent.BuildInfo, ciEvent.Project, ciEvent.BuildId);
				}
			}
			catch (InvalidCredentialException e)
			{
				Log.Error($"ReportEvent failed with TFS : {e.Message}");
				OctaneManagerInitializer.GetInstance().RestartPlugin();
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
