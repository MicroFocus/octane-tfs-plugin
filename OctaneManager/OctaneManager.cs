// (c) Copyright 2016 Hewlett Packard Enterprise Development LP
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using Hpe.Nga.Api.Core.Connector;
using Hpe.Nga.Api.Core.Connector.Exceptions;
using log4net;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using MicroFocus.Ci.Tfs.Octane.Dto;
using MicroFocus.Ci.Tfs.Octane.Dto.Connectivity;
using MicroFocus.Ci.Tfs.Octane.Dto.Events;
using MicroFocus.Ci.Tfs.Octane.Dto.General;
using MicroFocus.Ci.Tfs.Octane.Dto.Scm;
using MicroFocus.Ci.Tfs.Octane.Dto.TestResults;
using MicroFocus.Ci.Tfs.Octane.RestServer;
using MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems;
using MicroFocus.Ci.Tfs.Octane.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class OctaneManager
	{
		#region Const Defenitions

		private const int DEFAULT_POLLING_GET_TIMEOUT = 20 * 1000; //20 seconds
		private const int DEFAULT_POLLING_INTERVAL = 5;

		#endregion

		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly RestConnector _restConnector = new RestConnector();
		private readonly TfsManager _tfsManager;
		private static ConnectionDetails _connectionConf;

		private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(DEFAULT_POLLING_INTERVAL);
		private readonly int _pollingGetTimeout;
		private readonly UriResolver _uriResolver;
		private Task _taskPollingThread;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private readonly Server _server = new Server();
		//private CancellationToken _polingCancelationToken;
		private readonly TaskProcessor _taskProcessor;
		private readonly Uri _tfsServerURi;
		public OctaneManager(int pollingTimeout = DEFAULT_POLLING_GET_TIMEOUT)
		{
			_pollingGetTimeout = pollingTimeout;
			var hostName = Dns.GetHostName();
			var domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
			_connectionConf = ConfigurationManager.Read();
			_tfsServerURi = new Uri(_connectionConf.TfsLocation);
			var instanceDetails = new InstanceDetails(_connectionConf.InstanceId, _tfsServerURi.ToString());

			_uriResolver = new UriResolver(_connectionConf.SharedSpace, instanceDetails, _connectionConf);
			_tfsManager = new TfsManager(_connectionConf.Pat);
			_taskProcessor = new TaskProcessor(_tfsManager);
			Log.Debug("Octane manager created...");
		}

		public void SendTestResults(TfsBuildInfo buildInfo, string projectCiId, string buildCiId)
		{
			try
			{
				if (GetTestResultRelevant(projectCiId))
				{

					var run = _tfsManager.GetRunForBuid(buildInfo.CollectionName, buildInfo.Project, buildInfo.BuildId);
					var testResults = _tfsManager.GetTestResultsForRun(buildInfo.CollectionName, buildInfo.Project, run.Id.ToString());
					OctaneTestResult octaneTestResult = TestResultUtils.ConvertToOctaneTestResult(_connectionConf.InstanceId.ToString(), projectCiId, buildCiId, testResults, run.WebAccessUrl);
					String xml = TestResultUtils.SerializeToXml(octaneTestResult);

					ResponseWrapper res = _restConnector.ExecutePost(_uriResolver.GetTestResults(), null, xml,
						 RequestConfiguration.Create().SetGZipCompression(true).AddHeader("ContentType", "application/xml"));
					Log.Debug($"{buildInfo} : testResults are sent");
				}
				else
				{
					Log.Debug($"{buildInfo} - GetTestResultRelevant=false for project {projectCiId}");
				}
			}
			catch (Exception ex)
			{
				Log.Error($"{buildInfo} : error in SendTestResults : {ex.Message}");
			}
		}

		private bool GetTestResultRelevant(string jobName)
		{
			bool result = false;
			ResponseWrapper res = _restConnector.ExecuteGet(_uriResolver.GetTestResultRelevant(jobName), null);
			if (res.StatusCode == HttpStatusCode.OK)
			{
				result = Boolean.Parse(res.Data);
			}

			return result;
		}

		public void ShutDown()
		{
			_cancellationTokenSource.Cancel();
			_restConnector.Disconnect();
			IsInitialized = false;
		}

		public void WaitShutdown()
		{
			_taskPollingThread.Wait();
		}

		private void InitTaskPolling()
		{
			Log.Debug("Start init of polling thread");
			_taskPollingThread = Task.Factory.StartNew(() => PollOctaneTasks(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
		}

		public bool IsInitialized { get; protected set; } = false;

		private void PollOctaneTasks(CancellationToken token)
		{
			do
			{
				ResponseWrapper res = null;
				try
				{
					res =
						_restConnector.ExecuteGet(_uriResolver.GetTasksUri(), _uriResolver.GetTaskQueryParams(),
							RequestConfiguration.Create().SetTimeout(_pollingGetTimeout));
				}
				catch (Exception ex)
				{
					if (ex is WebException)
					{
						var innerEx = (WebException)ex.InnerException;
						var mqmEx = ex as MqmRestException;
						if (innerEx?.Status == WebExceptionStatus.Timeout)
						{
							Trace.WriteLine("Timeout");
							Trace.WriteLine($"Exception Info {mqmEx?.Description}");
						}
						else
						{
							Trace.WriteLine("Error getting status from octane : " + ex.Message);
						}
					}
					else
					{
						Trace.TraceError($"Known excetion  {ex}");
					}
				}
				finally
				{
					Trace.WriteLine($"Time: {DateTime.UtcNow.ToLongTimeString()}");
					if (res == null)
					{
						Trace.WriteLine("No tasks");
					}
					else
					{
						Trace.WriteLine("Received Task:");
						Trace.WriteLine($"Get status : {res?.StatusCode}");
						Trace.WriteLine($"Get data : {res?.Data}");
						try
						{
							var octaneTask = JsonHelper.DeserializeObject<OctaneTask>(res?.Data.TrimStart('[').TrimEnd(']'));
							var taskOutput = _taskProcessor.ProcessTask(octaneTask.Method, octaneTask?.ResultUrl);
							int status = HttpMethodEnum.POST.Equals(octaneTask.Method) ? 201 : 200;
							var response = new OctaneTaskResult(status, octaneTask.Id, taskOutput);

							if (octaneTask == null)
							{
								Trace.TraceError("Octane task was not json parsed , Nothing to send....");
							}
							else
							{
								if (!SendTaskResultToOctane(octaneTask.Id, response.ToString()))
								{
									Log.Error("Error sending results!");
								}
							}

						}
						catch (Exception ex)
						{
							Log.Error("Error deserializing Octane message!", ex);
						}
					}


				}
				Thread.Sleep(_pollingInterval);
			} while (!token.IsCancellationRequested);
		}

		private bool SendTaskResultToOctane(Guid resultId, string resultObj)
		{
			Log.Debug("Sending result to octane");
			Log.Debug(JsonHelper.FormatAsJsonObject(resultObj));
			try
			{
				var res = _restConnector.ExecutePut(_uriResolver.PostTaskResultUri(resultId.ToString()), null,
					resultObj);

				if (res.StatusCode == HttpStatusCode.NoContent)
				{
					return true;
				}

				Log.Debug($"Error sending result {resultId} with object {resultObj} to server");
				Log.Debug($"Error desc: {res?.StatusCode}, {res?.Data}");
			}
			catch (Exception ex)
			{
				Log.Error($"Error sending result {resultId} with object {resultObj} to server");
				Log.Error($"Error desc: {ex.Message}");
			}

			return false;
		}
		public void Init()
		{
			InitializeRestServer();
			InitializeConnectionToOctane();
			InitTaskPolling();

			IsInitialized = true;
		}

		private void InitializeRestServer()
		{
			_server.Start();
			RestBase.BuildEvent += RestBase_BuildEvent;
		}

		private void InitializeConnectionToOctane()
		{
			var connected = _restConnector.Connect(_connectionConf.Host,
				new APIKeyConnectionInfo(_connectionConf.ClientId, _connectionConf.ClientSecret));
			if (!connected)
			{
				throw new Exception("Could not connect to octane webapp");
			}
		}

		private void RestBase_BuildEvent(object sender, CiEvent finishEvent)
		{

			Log.Debug($"{finishEvent.BuildInfo} - start handling event");

			var list = new CiEventsList();
			list.Events.Add(CreateStartEvent(finishEvent));
			list.Events.Add(finishEvent);


			var scmData = GetScmData(finishEvent.BuildInfo);
			if (scmData != null)
			{
				list.Events.Add(CreateScmEvent(finishEvent, scmData));
				Log.Debug($"{finishEvent.BuildInfo} - scm data contains {scmData.Commits.Count} commits");
			}
			else
			{
				Log.Debug($"{finishEvent.BuildInfo} - scm data is empty");
			}

			list.Server = new CiServerInfo
			{
				Url = _tfsServerURi,
				InstanceId = _connectionConf.InstanceId,
				SendingTime = TestResultUtils.ConvertToOctaneTime(DateTime.UtcNow),
				InstanceIdFrom = TestResultUtils.ConvertToOctaneTime(DateTime.UtcNow)
			};

			var body = JsonHelper.SerializeObject(list);
			var res = _restConnector.ExecutePut(_uriResolver.GetEventsUri(), null, body);

			if (res.StatusCode == HttpStatusCode.OK)
			{
				Log.Debug($"{finishEvent.BuildInfo} - {list.Events.Count} events succesfully sent");
				SendTestResults(finishEvent.BuildInfo, finishEvent.Project, finishEvent.BuildId);
			}
			else
			{
				Log.Error("{finishEvent.BuildInfo} - events was not sent succesfully.");
			}

		}

		public ScmData GetScmData(TfsBuildInfo buildInfo)
		{
			try
			{
				ScmData scmData = null;
				var originalChanges = _tfsManager.GetBuildChanges(buildInfo.CollectionName, buildInfo.Project, buildInfo.BuildId);
				if (originalChanges.Count > 0)
				{
					var build = _tfsManager.GetBuild(buildInfo.CollectionName, buildInfo.Project, buildInfo.BuildId);
					ICollection<TfsScmChange> filteredChanges = GetFilteredBuildChanges(buildInfo, build, originalChanges);
					if (filteredChanges.Count > 0)
					{
						scmData = new ScmData();
						var repository = _tfsManager.GetRepositoryById(buildInfo.CollectionName, build.Repository.Id);
						scmData.Repository = new ScmRepository();
						scmData.Repository.Branch = build.SourceBranch;
						scmData.Repository.Type = build.Repository.Type;
						scmData.Repository.Url = repository.RemoteUrl;

						scmData.BuiltRevId = build.SourceVersion;
						scmData.Commits = new List<ScmCommit>();
						foreach (TfsScmChange change in filteredChanges)
						{
							var tfsCommit = _tfsManager.GetCommitWithChanges(change.Location);
							ScmCommit scmCommit = new ScmCommit();
							scmData.Commits.Add(scmCommit);
							scmCommit.User = tfsCommit.Committer.Name;
							scmCommit.UserEmail = tfsCommit.Committer.Email;
							scmCommit.Time = TestResultUtils.ConvertToOctaneTime(tfsCommit.Committer.Date);
							scmCommit.RevId = tfsCommit.CommitId;
							if (tfsCommit.Parents.Count > 0)
							{
								scmCommit.ParentRevId = tfsCommit.Parents[0];
							}

							scmCommit.Comment = tfsCommit.Comment;
							scmCommit.Changes = new List<ScmCommitFileChange>();

							foreach (var tfsCommitChange in tfsCommit.Changes)
							{
								if (!tfsCommitChange.Item.IsFolder)
								{
									ScmCommitFileChange commitChange = new ScmCommitFileChange();
									scmCommit.Changes.Add(commitChange);

									commitChange.Type = tfsCommitChange.ChangeType;
									commitChange.File = tfsCommitChange.Item.Path;
								}
							}
						}
					}

				}
				return scmData;

			}
			catch (Exception e)
			{
				Log.Error($"{buildInfo} - Failed to create scm data : {e.Message}");
				return null;
			}
		}

		/// <summary>
		/// Tfs returns associated changes from last successful build. That mean, for failed build it can return change that was reported for previous failed build.
		/// This method - clear previously reported changes of previous failed build
		/// </summary>
		private ICollection<TfsScmChange> GetFilteredBuildChanges(TfsBuildInfo buildInfo, TfsBuild build, ICollection<TfsScmChange> changes)
		{

			//put changes in map
			Dictionary<string, TfsScmChange> changesMap = new Dictionary<string, TfsScmChange>();
			foreach (TfsScmChange change in changes)
			{
				changesMap[change.Id] = change;
			}

			//find previous failed build
			IList<TfsBuild> previousBuilds = _tfsManager.GetPreviousFailedBuilds(buildInfo.CollectionName, buildInfo.Project, build.StartTime);
			TfsBuild foundPreviousFailedBuild = null;
			foreach (TfsBuild previousBuild in previousBuilds)
			{
				//pick only build that done on the same branch
				if (build.SourceBranch.Equals(previousBuild.SourceBranch))
				{
					foundPreviousFailedBuild = previousBuild;
					break;
				}
			}

			if (foundPreviousFailedBuild != null)
			{
				//remove changes from previous build
				var previousChanges = _tfsManager.GetBuildChanges(buildInfo.CollectionName, buildInfo.Project, foundPreviousFailedBuild.Id.ToString());
				foreach (TfsScmChange previousChange in previousChanges)
				{
					changesMap.Remove(previousChange.Id);
				}

				int removedCount = changes.Count - changesMap.Count;
				if (removedCount == 0)
				{
					Log.Debug($"{buildInfo} - build {build.Id} contains {changes.Count} associated changes. No one of them was already reported in previous build {foundPreviousFailedBuild.Id}");
				}
				else
				{
					Log.Debug($"{buildInfo} - build {build.Id} contains {changes.Count} associated changes while {removedCount} changes were already reported in build {foundPreviousFailedBuild.Id}");
				}
			}

			return changesMap.Values;

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

		public void ReportStartEvent(CiEvent startEvent)
		{
			startEvent.EventType = CiEventType.Started;

		}
		public void ReportFinishEvent(CiEvent finishEvent)
		{
			finishEvent.EventType = CiEventType.Finished;
		}

	}
}
