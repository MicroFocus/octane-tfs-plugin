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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		public OctaneManager(int servicePort, int pollingTimeout = DEFAULT_POLLING_GET_TIMEOUT)
		{
			_pollingGetTimeout = pollingTimeout;
			var hostName = Dns.GetHostName();
			var domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
			_connectionConf = ConfigurationManager.Read();
			_tfsServerURi = new Uri($"http://{hostName}:{servicePort}");
			var instanceDetails = new InstanceDetails(_connectionConf.InstanceId, _tfsServerURi.ToString());

			_uriResolver = new UriResolver(_connectionConf.SharedSpace, instanceDetails, _connectionConf);
			_tfsManager = new TfsManager(_connectionConf.Pat);
			_taskProcessor = new TaskProcessor(_tfsManager);
			Log.Debug("Octane manager created...");
		}

		public void SendTestResults(string tfsCollectionName, string tfsProject, string tfsBuildId, string projectCiId, string buildCiId)
		{
			if (GetTestResultRelevant(projectCiId))
			{
				try
				{
					var run = _tfsManager.GetRunForBuid(tfsCollectionName, tfsProject, tfsBuildId);
					var testResults = _tfsManager.GetTestResultsForRun(tfsCollectionName, tfsProject, run.Id.ToString());
					OctaneTestResult octaneTestResult = TestResultUtils.ConvertToOctaneTestResult(_connectionConf.InstanceId.ToString(), projectCiId, buildCiId, testResults, run.WebAccessUrl);
					String xml = TestResultUtils.SerializeToXml(octaneTestResult);

					ResponseWrapper res = _restConnector.ExecutePost(_uriResolver.GetTestResults(), null, xml,
						 RequestConfiguration.Create().SetGZipCompression(true).AddHeader("ContentType", "application/xml"));
				}
				catch (Exception ex)
				{
					Log.Error($"Error sending SendTestResults to server");
					Log.Error($"Error desc: {ex.Message}");
				}
			}
		}

		private bool GetTestResultRelevant(string jobName)
		{
			bool result = false;
			Log.Debug("Sending IsTestResultRelevant");
			try
			{
				ResponseWrapper res = _restConnector.ExecuteGet(_uriResolver.GetTestResultRelevant(jobName), null);
				if (res.StatusCode == HttpStatusCode.OK)
				{
					result = Boolean.Parse(res.Data);
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Error sending GetTestResultRelevant with jobname {jobName} to server");
				Log.Error($"Error desc: {ex.Message}");
			}

			Log.Debug($"IsTestResultRelevant - {jobName} : {result}");
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
							var octaneTask = JsonConvert.DeserializeObject<OctaneTask>(res?.Data.TrimStart('[').TrimEnd(']'));
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
			Log.Debug(JToken.Parse(resultObj).ToString());
			try
			{
				var res = _restConnector.ExecutePut(_uriResolver.PostTaskResultUri(resultId.ToString()), null,
					resultObj);

				if (res.StatusCode == HttpStatusCode.OK)
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

		private void RestBase_BuildEvent(object sender, Dto.CiEvent finishEvent)
		{
			string[] projectParts = finishEvent.Project.Split('.');
			string collectionName = projectParts[0];
			string project = projectParts[1];
			string buildDefinitionId = projectParts[2];

			var list = new CiEventsList();
			list.Events.Add(CreateStartEvent(finishEvent));
			list.Events.Add(finishEvent);

			string[] buildCiIdParts = finishEvent.BuildId.Split('.');
			string tfsBuildId = buildCiIdParts[buildCiIdParts.Length - 1];//tfsBuild is last part

			var scmData = GetScmData(collectionName, project, tfsBuildId);
			if (scmData != null)
			{
				list.Events.Add(CreateScmEvent(finishEvent, scmData));
			}

			list.Server = new CiServerInfo
			{
				Url = _tfsServerURi,
				InstanceId = _connectionConf.InstanceId,
				SendingTime = TestResultUtils.ConvertToOctaneTime(DateTime.UtcNow),
				InstanceIdFrom = TestResultUtils.ConvertToOctaneTime(DateTime.UtcNow)
			};

			var body = JsonConvert.SerializeObject(list);
			var res = _restConnector.ExecutePut(_uriResolver.GetEventsUri(), null, body);

			if (res.StatusCode == HttpStatusCode.OK)
			{
				Log.Info("Event succesfully sent");
				SendTestResults(collectionName, project, tfsBuildId, finishEvent.Project, finishEvent.BuildId);
			}
			else
			{
				Log.Error("Event was not sent succesfully");
			}

		}

		public ScmData GetScmData(string collectionName, string project, string buildNumber)
		{
			try
			{
				ScmData scmData = null;
				var changes = _tfsManager.GetBuildChanges(collectionName, project, buildNumber);
				if (changes.Count > 0)
				{
					var build = _tfsManager.GetBuild(collectionName, project, buildNumber);
					ICollection<TfsScmChange> filteredChanges = GetFilteredBuildChanges(collectionName, project, build, changes);
					if (filteredChanges.Count > 0)
					{
						scmData = new ScmData();
						var repository = _tfsManager.GetRepositoryById(collectionName, build.Repository.Id);
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
				Log.Error($"Failed to create scm data for {collectionName}.{project}.{buildNumber}-{e.Message}");
				return null;
			}
		}

		/// <summary>
		/// Tfs returns associated changes from last successful build. That mean, for failed build it can return change that was reported for previous failed build.
		/// This method - clear previously reported changes of previous failed build
		/// </summary>
		private ICollection<TfsScmChange> GetFilteredBuildChanges(string collectionName, string project, TfsBuild build, ICollection<TfsScmChange> changes)
		{
			if (build.Result.Equals("failed"))
			{
				//put changes in map
				Dictionary<string, TfsScmChange> changesMap = new Dictionary<string, TfsScmChange>();
				foreach (TfsScmChange change in changes)
				{
					changesMap[change.Id] = change;
				}

				//find previous failed build
				IList<TfsBuild> previousBuilds = _tfsManager.GetPreviousFailedBuilds(collectionName, project, build.StartTime);
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
					var previousChanges = _tfsManager.GetBuildChanges(collectionName, project, foundPreviousFailedBuild.Id.ToString());
					foreach (TfsScmChange previousChange in previousChanges)
					{
						changesMap.Remove(previousChange.Id);
					}

					int removedCount = changes.Count - changesMap.Count;
					if (removedCount == 0)
					{
						Log.Debug($"Build {build.Id} contains {changes.Count} associated changes. No one of them was already reported in previous build {foundPreviousFailedBuild.Id}");
					}
					else
					{
						Log.Debug($"Build {build.Id} contains {changes.Count} associated changes while {removedCount} changes were already reported in build {foundPreviousFailedBuild.Id}");
					}
				}

				return changesMap.Values;
			}
			else
			{
				return changes;
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

	}
}
