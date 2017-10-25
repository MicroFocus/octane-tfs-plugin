using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MicroFocus.Ci.Tfs.Octane.dto;
using MicroFocus.Ci.Tfs.Octane.dto.general;
using MicroFocus.Ci.Tfs.Octane.dto.pipelines;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Client;

namespace MicroFocus.Ci.Tfs.Octane
{
    public class TfsManager
    {
        private static readonly CiJobList MockJobList = new CiJobList();
        private readonly TfsConfigurationServer _configurationServer;
        private static readonly string _tfsUrl = "http://localhost:8080/tfs";
        public TfsManager()
        {
            MockJobList.Jobs.Add(new PipelineNode("e7c63454-f81d-4786-8a33-cc1e7c9fa5ce", "Tfs Test Job"));
            MockJobList.Jobs.Add(new PipelineNode("f9e21042-a286-44bf-808a-8c9462cb3666", "Tfs Test Job 2"));
            MockJobList.Jobs.Add(new PipelineNode("d45f798c-ccf7-48dc-970d-3b320d63c75c", "Tfs Test Job 3"));

            Uri tfsUri = new Uri(_tfsUrl);

            _configurationServer =
                TfsConfigurationServerFactory.GetConfigurationServer(tfsUri);
        }

        public IDtoBase GetJobsList()
        {
            //TODO: Change this to real code , currently a mockup!            

            Uri configurationServerUri = new Uri("http://localhost:8080/tfs");
            TfsConfigurationServer configurationServer =
                TfsConfigurationServerFactory.GetConfigurationServer(configurationServerUri);

            ITeamProjectCollectionService tpcService = configurationServer.GetService<ITeamProjectCollectionService>();

            foreach (TeamProjectCollection tpc in tpcService.GetCollections())
            {
                Trace.WriteLine("Team project collection");
            }

            return MockJobList;
        }

        public IDtoBase GetJobDetail(string jobId)
        {
            return MockJobList[jobId];
        }     

        public void ListProjectsInCollection()
        {            
            // Get the catalog of team project collections
            ReadOnlyCollection<CatalogNode> collectionNodes = _configurationServer.CatalogNode.QueryChildren(
                new[] { CatalogResourceTypes.ProjectCollection },
                false, CatalogQueryOptions.None);
            
            // List the team project collections
            foreach (CatalogNode collectionNode in collectionNodes)
            {
                
                // Use the InstanceId property to get the team project collection
                Guid collectionId = new Guid(collectionNode.Resource.Properties["InstanceId"]);
                TfsTeamProjectCollection teamProjectCollection = _configurationServer.GetTeamProjectCollection(collectionId);

                // Print the name of the team project collection
                Console.WriteLine("Collection: " + teamProjectCollection.Name);

                // Get a catalog of team projects for the collection
                ReadOnlyCollection<CatalogNode> projectNodes = collectionNode.QueryChildren(
                    new[] { CatalogResourceTypes.TeamProject },
                    false, CatalogQueryOptions.None);

                // List the team projects in the collection
                foreach (CatalogNode projectNode in projectNodes)
                {
                    
                    Console.WriteLine(" Team Project: " + projectNode.Resource.DisplayName);
                    //var res = GetBuildDefinitionListFromProject(collectionId, projectNode.Resource.DisplayName);
                    GetBuildDefenition(collectionNode.Resource.DisplayName, projectNode.Resource.DisplayName);

                }
            }
        }

        public IList<KeyValuePair<string, Uri>> GetBuildDefinitionListFromProject(Guid collectionId, string projectName)
        {
            List<IBuildDefinition> buildDefinitionList = null;
            List<KeyValuePair<string, Uri>> buildDefinitionInfoList = null;

            try
            {
                buildDefinitionInfoList = new List<KeyValuePair<string, Uri>>();
                TfsTeamProjectCollection tfsProjectCollection =
                    _configurationServer.GetTeamProjectCollection(collectionId);
                //tfsProjectCollection.Authenticate();
                var buildServer = (IBuildServer)tfsProjectCollection.GetService(typeof(IBuildServer));
                buildDefinitionList = new List<IBuildDefinition>(buildServer.QueryBuildDefinitions(projectName));
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
            }

            if (buildDefinitionList != null && buildDefinitionList.Count > 0)
            {
                foreach (IBuildDefinition builddef in buildDefinitionList)
                {
                    buildDefinitionInfoList.Add(new KeyValuePair<string, Uri>(builddef.Name, builddef.Uri));
                }
            }
            return buildDefinitionInfoList;
        }

        public void GetBuildDefenition(string collectionName,string projectName)
        {
            var tfsUrl = $"http://{_tfsUrl}/{collectionName}";
            var buildClient = new BuildHttpClient(new Uri(tfsUrl), new VssAadCredential());
            var definitions = buildClient.GetDefinitionsAsync(project: projectName);
            //var builds = buildClient.GetBuildsAsync("<projectname>";

            foreach (var buildDefenition in definitions.Result)
            {
                Trace.WriteLine(buildDefenition.Name);
            }
//            foreach (var build in builds.Result)
//            {
//                Console.WriteLine(String.Format("{0} - {1} - {2} - {3}", build.Definition.Name, build.Id.ToString(), build.Status.ToString(), build.StartTime.ToString()));
//            }
        }
    }
}
