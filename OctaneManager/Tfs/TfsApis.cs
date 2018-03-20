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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.General;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Pipelines;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.ApiItems;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.v1;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.v1.SCM;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Contexts;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs
{
	public class TfsApis
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private CiJobList _cachedJobList = new CiJobList();

		protected readonly TfsSubscriptionManager _subscriptionManager;
		private readonly TfsConfiguration _tfsConf;
		private readonly TfsRestConnector _tfsRestConnector;

		//private const string TfsUrl = "http://localhost:8080/tfs";

		public TfsApis(string tfsLocation, string pat)
		{
			string myTfsLocation = tfsLocation.EndsWith("/") ? tfsLocation : tfsLocation + "/";
			_tfsConf = new TfsConfiguration(new Uri(myTfsLocation), pat);
			_tfsRestConnector = new TfsRestConnector(_tfsConf);
			_subscriptionManager = new TfsSubscriptionManager(_tfsConf);
		}

		public Uri TfsUri
		{
			get
			{
				return _tfsConf.Uri;
			}
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
						var id = OctaneUtils.GenerateOctaneJobCiId(collection.Name, project.Id, buildDefinition.Id);
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

			if (RunModeManager.GetInstance().RunMode == PluginRunMode.ConsoleApp)
			{
				var tfsCiEntity = OctaneUtils.TranslateOctaneJobCiIdToObject(jobId);
				if (!_subscriptionManager.SubscriptionExists(tfsCiEntity.CollectionName, tfsCiEntity.ProjectId))
				{
					_subscriptionManager.AddSubscription(tfsCiEntity.CollectionName, tfsCiEntity.ProjectId);
				}
			}

			return res;
		}

		public TfsBuild QueueNewBuild(string collectionName, string projectId, string buildDefinitionId)
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/build/builds#queueabuild
			var uriSuffix = ($"{collectionName}/{projectId}/_apis/build/builds/?api-version=2.0");
			var body = $"{{\"definition\": {{ \"id\": {buildDefinitionId}}}}}";
			var build = _tfsRestConnector.SendPost<TfsBuild>(uriSuffix, body);
			return build;
		}

		public List<TfsScmChange> GetBuildChanges(string collectionName, string projectId, string buildId)
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/build/builds#changes
			var uriSuffix = ($"{collectionName}/{projectId}/_apis/build/builds/{buildId}/changes?api=version=2.0");
			const int pageSize = 200;
			const int maxPages = 5;
			var changes = _tfsRestConnector.GetPagedCollection<TfsScmChange>(uriSuffix, pageSize, maxPages, null);
			return changes;
		}

		public TfsScmCommit GetCommitWithChanges(string commitUrl)
		{
			var urlWithChanges = commitUrl;
			//https://www.visualstudio.com/en-us/docs/integrate/api/git/commits#with-changed-items
			if (!commitUrl.Contains("changeCount"))
			{
				var joiner = commitUrl.Contains("?") ? "&" : "?";
				urlWithChanges = $"{commitUrl}{joiner}changeCount=100";
			}

			var commit = _tfsRestConnector.SendGet<TfsScmCommit>(urlWithChanges);
			return commit;
		}

		public TfsScmRepository GetRepositoryByLocation(string repositoryUrl)
		{
			var repository = _tfsRestConnector.SendGet<TfsScmRepository>(repositoryUrl);
			return repository;
		}

		public TfsScmRepository GetRepositoryById(string collectionName, string repositoryId)
		{
			var url = $"{collectionName}/_apis/git/repositories/{repositoryId}";
			var repository = _tfsRestConnector.SendGet<TfsScmRepository>(url);
			return repository;
		}

		public IList<TfsTestResult> GetTestResultsForRun(string collectionName, string projectName, string runId)
		{
			var url = $"{collectionName}/{projectName}/_apis/test/runs/{runId}/results?api-version=1.0";
			const int pageSize = 1000;
			const int maxPages = 100;

			var testResults = _tfsRestConnector.GetPagedCollection<TfsTestResult>(url, pageSize, maxPages, LogUtils.TFS_TEST_RESULTS_LOGGER);
			return testResults;
		}

		public TfsRun GetRunForBuild(string collectionName, string projectName, string buildId)
		{
			//var build = GetBuild(collectionName, projectName, buildId);
			var buildUri = "vstfs:///Build/Build/" + buildId;
			var uriSuffix = ($"{collectionName}/{projectName}/_apis/test/runs?api-version=1.0&buildUri={buildUri}");
			var runs = _tfsRestConnector.GetCollection<TfsRun>(uriSuffix);
			return runs.Count > 0 ? runs[0] : null;
		}

		public TfsBuild GetBuild(string collectionName, string projectId, string buildId)
		{
			var uriSuffix = ($"{collectionName}/{projectId}/_apis/build/builds/{buildId}?api-version=2.0");
			var build = _tfsRestConnector.SendGet<TfsBuild>(uriSuffix);
			return build;
		}

		public IList<TfsBuild> GetPreviousFailedBuilds(string collectionName, string projectId, string maxFinishTime)
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/build/builds
			var uriSuffix = ($"{collectionName}/{projectId}/_apis/build/builds?api-version=2.0&resultFilter=failed&maxFinishTime={maxFinishTime}&$top=100");
			var builds = _tfsRestConnector.GetCollection<TfsBuild>(uriSuffix);
			return builds;
		}

		public IList<TfsProjectCollection> GetProjectCollections()
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/tfs/project-collections
			var uriSuffix = ($"_apis/projectcollections?api-version=1.0");
			var collections = _tfsRestConnector.GetCollection<TfsProjectCollection>(uriSuffix);
			return collections;
		}

		public void ConnectionValidation(string context)
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/tfs/project-collections
			var uriSuffix = ($"_apis/projectcollections?api-version=1.0&context={context}");
			_tfsRestConnector.GetCollection<TfsProjectCollection>(uriSuffix);
		}

		private IList<TfsProject> GetProjects(string collectionName)
		{		    		 
            //https://www.visualstudio.com/en-us/docs/integrate/api/tfs/projects
            var uriSuffix = ($"{collectionName}/_apis/projects?api-version=1.0");
			var collections = _tfsRestConnector.SendGet<TfsBaseCollection<TfsProject>>(uriSuffix);
			return collections.Items;
		}

		private IList<TfsBuildDefinition> GetBuildDefinitions(string collectionName, string projectName)
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/xamlbuild/definitions
			var uriSuffix = ($"{collectionName}/{projectName}/_apis/build/definitions?api-version=2.0");
			var definitions = _tfsRestConnector.GetCollection<TfsBuildDefinition>(uriSuffix);
			return definitions;
		}

	}
}
