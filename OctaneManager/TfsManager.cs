using log4net;
using MicroFocus.Ci.Tfs.Octane.dto;
using MicroFocus.Ci.Tfs.Octane.dto.general;
using MicroFocus.Ci.Tfs.Octane.dto.pipelines;
using MicroFocus.Ci.Tfs.Octane.Tfs;
using MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1.SCM;
using MicroFocus.Ci.Tfs.Octane.Tools;
using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MicroFocus.Ci.Tfs.Octane
{
	public class TfsManager
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly CiJobList MockJobList = new CiJobList();

	
		private CiJobList _cachedJobList = new CiJobList();

		protected readonly SubscriptionManager _subscriptionManager;
		private readonly TfsConfiguration _tfsConf;
		private readonly TfsHttpConnector _tfsConnector;
		private readonly TfsConfigurationServer _configurationServer;
		private const string TfsUrl = "http://localhost:8080/tfs/";

		public TfsManager(string pat)
		{
			_tfsConf = new TfsConfiguration(new Uri(TfsUrl), pat);
			_tfsConnector = new TfsHttpConnector(_tfsConf);
			_configurationServer = TfsConfigurationServerFactory.GetConfigurationServer(_tfsConf.Uri);
			_subscriptionManager = new SubscriptionManager(_tfsConf);
		}

		public IDtoBase GetJobsList()
		{
			var result = new CiJobList();
			var collections = GetProjectCollections();

			//TODO: make this simplier
			foreach (var collection in collections)
			{
				var projects = GetProjects(collection.Name);
				foreach (var project in projects)
				{
					var buildDefinitions = GetBuildDefinitions(collection.Name, project.Id);
					foreach (var buildDefinition in buildDefinitions)
					{
						var id = PipelineNode.GenerateOctaneJobCiId(collection.Name, project.Id, buildDefinition.Id);
						Log.Debug($"New job added to list with id: {id}");
						result.Jobs.Add(new PipelineNode(id, buildDefinition.Name));
					}
				}
			}
			_cachedJobList = result;

			return _cachedJobList;
		}

		public IDtoBase GetJobDetail(string jobId)
		{
			var res = _cachedJobList[jobId];
			if (res == null)
			{
				GetJobsList();
				res = _cachedJobList[jobId];
			}

			var tfsCiEntity = PipelineNode.TranslateOctaneJobCiIdToObject(jobId);
			if (!_subscriptionManager.SubscriptionExists(tfsCiEntity.CollectionName, tfsCiEntity.ProjectId))
			{
				_subscriptionManager.AddBuildCompletion(tfsCiEntity.CollectionName, tfsCiEntity.ProjectId);
			}

			return res;
		}

		public TfsBuild QueueNewBuild(string collectionName, string projectId, string buildDefinitionId)
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/build/builds#queueabuild
			var uriSuffix = ($"{collectionName}/{projectId}/_apis/build/builds/?api-version=2.0");
			var body = $"{{\"definition\": {{ \"id\": {buildDefinitionId}}}}}";
			var build = _tfsConnector.SendPost<TfsBuild>(uriSuffix, body);
			return build;
		}

		public List<TfsScmChange> GetBuildChanges(string collectionName, string projectId, string buildId)
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/build/builds#changes
			var uriSuffix = ($"{collectionName}/{projectId}/_apis/build/builds/{buildId}/changes?api=version=2.0");
			var changes = _tfsConnector.GetPagedCollection<TfsScmChange>(uriSuffix, 100);
			return changes;
		}

		public TfsScmCommit GetCommitWithChanges(string commitUrl)
		{
			var urlWithChanges = commitUrl;
			//https://www.visualstudio.com/en-us/docs/integrate/api/git/commits#with-changed-items
			if (!commitUrl.Contains("changeCount")){
				var joiner = commitUrl.Contains("?") ? "&" : "?";
				urlWithChanges = $"{commitUrl}{joiner}changeCount=100";
			}

			var commit = _tfsConnector.SendGet<TfsScmCommit>(urlWithChanges);
			return commit;
		}

		public TfsScmRepository GetRepositoryByLocation(string repositoryUrl)
		{
			var repository = _tfsConnector.SendGet<TfsScmRepository>(repositoryUrl);
			return repository;
		}

		public TfsScmRepository GetRepositoryById(string collectionName, string repositoryId)
		{
			var url = $"{collectionName}/_apis/git/repositories/{repositoryId}";
			var repository = _tfsConnector.SendGet<TfsScmRepository>(url);
			return repository;
		}

		

		public IList<TfsTestResult>  GetTestResultsForRun(string collectionName, string projectName, string runId)
		{
			var url = $"{collectionName}/{projectName}/_apis/test/runs/{runId}/results?api-version=1.0";
			const int pageSize = 1000;
			var testResults = _tfsConnector.GetPagedCollection<TfsTestResult>(url, pageSize);
			
			return testResults;
		}

		public TfsRun GetRunForBuid(string collectionName, string projectName, string buildId)
		{
			var build = GetBuild(collectionName, projectName, buildId);
			var uriSuffix = ($"{collectionName}/{projectName}/_apis/test/runs?api-version=1.0&buildUri={build.Uri}");
			var runs = _tfsConnector.GetCollection<TfsRun>(uriSuffix);
			return runs.Count > 0 ? runs[0] : null;
		}

		public TfsBuild GetBuild(string collectionName, string projectId, string buildId)
		{
			var uriSuffix = ($"{collectionName}/{projectId}/_apis/build/builds/{buildId}?api-version=2.0");
			var build = _tfsConnector.SendGet<TfsBuild>(uriSuffix);
			return build;
		}

		public IList<TfsBuild> GetPreviousFailedBuilds(string collectionName, string projectId,string maxFinishTime)
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/build/builds
			var uriSuffix = ($"{collectionName}/{projectId}/_apis/build/builds?api-version=2.0&resultFilter=failed&maxFinishTime={maxFinishTime}&$top=100");
			var builds = _tfsConnector.GetCollection<TfsBuild>(uriSuffix);
			
			return builds;
		}

		private IList<TfsProjectCollection> GetProjectCollections()
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/tfs/project-collections
			var uriSuffix = ($"_apis/projectcollections?api-version=1.0");
			var collections = _tfsConnector.GetCollection<TfsProjectCollection>(uriSuffix);
			return collections;
		}

		private IList<TfsProject> GetProjects(string collectionName)
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/tfs/projects
			var uriSuffix = ($"{collectionName}/_apis/projects?api-version=1.0");
			var collections = _tfsConnector.SendGet<TfsBaseCollection<TfsProject>>(uriSuffix);
			return collections.Items;
		}

		private IList<TfsBuildDefinition> GetBuildDefinitions(string collectionName, string projectName)
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/xamlbuild/definitions
			var uriSuffix = ($"{collectionName}/{projectName}/_apis/build/definitions?api-version=2.0");
			var definitions = _tfsConnector.GetCollection<TfsBuildDefinition>(uriSuffix);
			return definitions;
		}

	}
}
