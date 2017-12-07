using Flurl;
using log4net;
using MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans;
using MicroFocus.Ci.Tfs.Octane.Tools;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace MicroFocus.Ci.Tfs.Octane.Tfs
{
	public abstract class TfsManagerBase
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	    protected readonly SubscriptionManager _subscriptionManager;
        private readonly TfsConfiguration _tfsConf;
		private readonly TfsHttpConnector _tfsConnector;
		private readonly TfsConfigurationServer _configurationServer;
	    

		private const string TfsUrl = "http://localhost:8080/tfs/";

		protected TfsManagerBase(string pat)
		{
			_tfsConf = new TfsConfiguration(new Uri(TfsUrl), pat);
			_tfsConnector = new TfsHttpConnector(_tfsConf);
			_configurationServer =
				TfsConfigurationServerFactory.GetConfigurationServer(_tfsConf.Uri);
            _subscriptionManager= new SubscriptionManager(_tfsConf);
		}

		protected List<TfsCollectionItem> GetCollections()
		{

			var visualStudioServicesConnection = new VssConnection(_tfsConf.Uri, new PatCredentials(string.Empty, _tfsConf.Pat));

			// get ahold of the Project Collection client
			var projectCollectionHttpClient = visualStudioServicesConnection.GetClient<ProjectCollectionHttpClient>();
			var result = new List<TfsCollectionItem>();
			// iterate over the first 10 Project Collections (I am allowed to see)
			// however, if no parameter(s) were provided to the .GetProjectCollections() method, it would only retrieve one Collection,
			// so basically this allows / provides fine-grained pagination control
			foreach (var projectCollectionReference in projectCollectionHttpClient.GetProjectCollections().Result)
			{
				// retrieve a reference to the actual project collection based on its (reference) .Id
				var projectCollection = projectCollectionHttpClient.GetProjectCollection(projectCollectionReference.Id.ToString()).Result;

				// the 'web' Url is the one for the PC itself, the API endpoint one is different, see below
				var webUrlForProjectCollection = projectCollection.Links.Links["web"] as ReferenceLink;

				if (webUrlForProjectCollection != null)
					Log.Debug(
						$"Project Collection '{projectCollection.Name}' (Id: {projectCollection.Id}) at Web Url: '{webUrlForProjectCollection.Href}' & API Url: '{projectCollection.Url}'");

				result.Add(new TfsCollectionItem(projectCollection.Id, projectCollection.Name));

			}

			return result;
		}

		protected List<TfsProjectItem> GetProjects(TfsCollectionItem collection)
		{
			return GetProjects(collection.Name);
		}

		protected List<TfsProjectItem> GetProjects(string collectionName)
		{
			var collectionUri = new Uri(Url.Combine(_tfsConf.Uri.ToString(), collectionName));
			var collectionVssConnection = new VssConnection(collectionUri, new PatCredentials(string.Empty, _tfsConf.Pat));
			var projectHttpClient = collectionVssConnection.GetClient<ProjectHttpClient>();

			var result = new List<TfsProjectItem>();
			foreach (var projectReference in projectHttpClient.GetProjects().Result)
			{
				// and then get ahold of the actual project
				var teamProject = projectHttpClient.GetProject(projectReference.Id.ToString()).Result;
				var urlForTeamProject = ((ReferenceLink)teamProject.Links.Links["web"]).Href;

				Trace.WriteLine(
					$"Team Project '{teamProject.Name}' (Id: {teamProject.Id}) at Web Url: '{urlForTeamProject}' & API Url: '{teamProject.Url}'");

				result.Add(new TfsProjectItem(teamProject.Id, teamProject.Name));
			}

			return result;
		}

		protected List<TfsBuildDefinitionItem> GetBuildDefinitions(TfsCollectionItem collection, TfsProjectItem project)
		{
			return GetBuildDefinitions(collection.Name, project.Name);
		}

		protected List<TfsBuildDefinitionItem> GetBuildDefinitions(string collectionName, string projectName)
		{
			var uri = _tfsConf.Uri.Append(collectionName);
			var buildClient = new BuildHttpClient(uri, new PatCredentials(string.Empty, _tfsConf.Pat));
			var definitions = buildClient.GetDefinitionsAsync(project: projectName);
			var result = new List<TfsBuildDefinitionItem>();
			foreach (var buildDefinition in definitions.Result)
			{
				Trace.WriteLine(buildDefinition.Name);

				result.Add(new TfsBuildDefinitionItem(buildDefinition.Id.ToString(), buildDefinition.Name));

			}

			return result;

		}

		public void QueueNewBuild(string collectionName, string projectId, string buildDefinitionId)
		{
			//https://www.visualstudio.com/en-us/docs/integrate/api/build/builds#queueabuild
			var uriSuffix = ($"{collectionName}/{projectId}/_apis/build/builds/?api-version=2.0");
			var body = $"{{\"definition\": {{ \"id\": {buildDefinitionId}}}}}";
			var build = _tfsConnector.SendPost<TfsBuild>(uriSuffix, body);
		}

		public TfsBuild GetBuild(string collectionName, string projectId, string buildId)
		{
			var uriSuffix = ($"{collectionName}/{projectId}/_apis/build/builds/{buildId}?api-version=1.0");
			var build = _tfsConnector.SendGet<TfsBuild>(uriSuffix);
			return build;
		}

		private TfsRun GetRunByBuildUri(string collectionName, string projectName, string buildUri)
		{
			var uriSuffix = ($"{collectionName}/{projectName}/_apis/test/runs?api-version=1.0&buildUri={buildUri}");
			var runs = _tfsConnector.SendGet<TfsRuns>(uriSuffix);
			return runs.Results.Count > 0 ? runs.Results[0] : null;
		}

		public TfsTestResults GetTestResultsByBuildUri(string collectionName, string projectName, String buildUri)
		{
			var run = GetRunByBuildUri(collectionName, projectName, buildUri);
			const int top = 1000;
			var skip = 0;
			var completed = false;
			TfsTestResults finalResults = null;

			while (!completed)
			{
				var uriSuffix = ($"{collectionName}/{projectName}/_apis/test/runs/{run.Id}/results?api-version=1.0&$skip={skip}&$top={top}");
				var results = _tfsConnector.SendGet<TfsTestResults>(uriSuffix);
				skip += top;
				completed = results.Count < top;
				if (finalResults == null)
				{
					finalResults = results;
				}
				else
				{
					finalResults.Join(results);
				}
			}

			finalResults.Run = run;
			return finalResults;
		}
	}
}
