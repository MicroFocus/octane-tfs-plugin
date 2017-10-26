﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans;
using MicroFocus.Ci.Tfs.Octane.Tools;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace MicroFocus.Ci.Tfs.Octane.Tfs
{
    public abstract class TfsManagerBase
    {
        private readonly TfsConfigurationServer _configurationServer;
        private const string TfsUrl = "http://localhost:8080/tfs";
        private readonly Uri _tfsUri;

        protected TfsManagerBase()
        {
            _tfsUri = new Uri(TfsUrl);


            _configurationServer =
                TfsConfigurationServerFactory.GetConfigurationServer(_tfsUri);
        }

        protected List<TfsCollectionItem> GetCollections()
        {
            var visualStudioServicesConnection = new VssConnection(_tfsUri, new VssCredentials());

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
                    Trace.WriteLine(
                        $"Project Collection '{projectCollection.Name}' (Id: {projectCollection.Id}) at Web Url: '{webUrlForProjectCollection.Href}' & API Url: '{projectCollection.Url}'");

                result.Add(new TfsCollectionItem(projectCollection.Id,projectCollection.Name));

            }

            return result;

            //            ReadOnlyCollection<CatalogNode> collectionNodes = _configurationServer.CatalogNode.QueryChildren(
            //                new[] {CatalogResourceTypes.ProjectCollection},
            //                false, CatalogQueryOptions.None);
            //
            //            var result = new List<TfsCollectionItem>();
            //            foreach (CatalogNode collectionNode in collectionNodes)
            //            {
            //
            //                // Use the InstanceId property to get the team project collection
            //                Guid collectionId = new Guid(collectionNode.Resource.Properties["InstanceId"]);
            //                TfsTeamProjectCollection teamProjectCollection =
            //                    _configurationServer.GetTeamProjectCollection(collectionId);
            //
            //                // Print the name of the team project collection
            //                Trace.WriteLine("Found Collection: " + teamProjectCollection.Name);
            //
            //                result.Add(new TfsCollectionItem(teamProjectCollection.InstanceId, teamProjectCollection.Name));
            //            }
            //
            //            return result;
        }

        protected List<TfsProjectItem> GetProjects(TfsCollectionItem collection)
        {
            return GetProjects(collection.Name);
        }

        protected List<TfsProjectItem> GetProjects(string collectionName)
        {
            var collectionUri = new Uri(Url.Combine(_tfsUri.ToString(), collectionName));
            VssConnection collectionVssConnection = new VssConnection(collectionUri, new VssCredentials());
            var projectHttpClient = collectionVssConnection.GetClient<ProjectHttpClient>();

            var result = new List<TfsProjectItem>();
            foreach (var projectReference in projectHttpClient.GetProjects().Result)
            {
                // and then get ahold of the actual project
                var teamProject = projectHttpClient.GetProject(projectReference.Id.ToString()).Result;
                var urlForTeamProject = ((ReferenceLink) teamProject.Links.Links["web"]).Href;

                Trace.WriteLine(
                    $"Team Project '{teamProject.Name}' (Id: {teamProject.Id}) at Web Url: '{urlForTeamProject}' & API Url: '{teamProject.Url}'");

                result.Add(new TfsProjectItem(teamProject.Id, teamProject.Name));
            }

            return result;
        }

        protected List<TfsBuildDefenitionItem> GetBuildDefenitions(TfsCollectionItem collection, TfsProjectItem project)
        {
            return GetBuildDefenitions(collection.Name, project.Name);
        }

        protected List<TfsBuildDefenitionItem> GetBuildDefenitions(string collectionName, string projectName)
        {            
            var uri = _tfsUri.Append(collectionName);        
            var buildClient = new BuildHttpClient(uri, new VssAadCredential());
            var definitions = buildClient.GetDefinitionsAsync(project: projectName);
            var result = new List<TfsBuildDefenitionItem>();
            foreach (var buildDefenition in definitions.Result)
            {
                Trace.WriteLine(buildDefenition.Name);

                result.Add(new TfsBuildDefenitionItem(buildDefenition.Id.ToString(), buildDefenition.Name));

            }

            return result;

        }
    }
}
