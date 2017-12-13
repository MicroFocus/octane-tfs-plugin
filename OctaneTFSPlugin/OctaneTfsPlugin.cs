using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane;
using Microsoft.TeamFoundation.Build.Server;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Build.WebApi.Events;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Server;

namespace MicroFocus.Ci.Tfs.Core
{
    public class OctaneTfsPlugin : ISubscriber
    {
        private static string PLUGIN_DISPLAY_NAME = "OctaneTfsPlugin";

        private readonly TimeSpan _initTimeout = new TimeSpan(0,0,0,5);

        private static OctaneManager _octaneManager =null;

        private static Task _octaneInitializationThread = null;
        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public OctaneTfsPlugin()
        {          
            if (_octaneInitializationThread == null)
            {
                _octaneInitializationThread =
                    Task.Factory.StartNew(() => InitializeOctaneManager(_cancellationTokenSource.Token),
                        TaskCreationOptions.LongRunning);
            }

        }

        private void InitializeOctaneManager(CancellationToken token)
        {
            while (!IsOctaneInitialized())
            {                 
                if (token.IsCancellationRequested)
                {
                    TeamFoundationApplicationCore.Log("Octane initialization thread was requested to quit!", 1, EventLogEntryType.Information);
                    break;
                }
                try
                {
                    _octaneManager = new OctaneManager();
                    _octaneManager.Init();
                    
                }
                catch (Exception ex)
                {
                    TeamFoundationApplicationCore.Log($"Error initializing octane plugin! {ex.Message}",1,EventLogEntryType.Error);
                }

                //Sleep till next retry
                Thread.Sleep(_initTimeout);

            }
        }

        private static bool IsOctaneInitialized()
        {
            return _octaneManager != null && _octaneManager.IsInitialized;
        }
        

        public Type[] SubscribedTypes()
        {
            var subscribedEventsList = new List<Type>()
            {
                typeof(BuildCompletionNotificationEvent),
                typeof(BuildStartedNotificationEvent)                
            };

            return subscribedEventsList.ToArray();
        }

        public string Name => PLUGIN_DISPLAY_NAME;
        public SubscriberPriority Priority => SubscriberPriority.Normal;

        public EventNotificationStatus ProcessEvent(IVssRequestContext requestContext, NotificationType notificationType,
            object notificationEventArgs, out int statusCode, out string statusMessage,
            out Microsoft.TeamFoundation.Common.ExceptionPropertyCollection properties)
        {
            statusCode = 0;
            properties = null;
            statusMessage = String.Empty;
            try
            {
                if (notificationType == NotificationType.Notification && notificationEventArgs is BuildCompletionNotificationEvent)
                {
                  
                  
                  Trace.WriteLine($"Processing notification of type \"{notificationType}\"");                    
                }

            }
            catch (Exception e)
            {
                
                
                Trace.TraceError($"An error \"{e.Message}\" occured during processing of notification.", e);
                
                TeamFoundationApplicationCore.Log(requestContext, "HPE ALI : Process Server Event",
                    $"The error occured during processing notification: {e}", 123, EventLogEntryType.Error);
            }
            return EventNotificationStatus.ActionPermitted;
        }

//        private void ListProjectsInCollection()
//        {
//            Uri tfsUri = new Uri("http://localhost:8080/tfs");
//
//            TfsConfigurationServer configurationServer =
//                TfsConfigurationServerFactory.GetConfigurationServer(tfsUri);
//
//            // Get the catalog of team project collections
//            ReadOnlyCollection<CatalogNode> collectionNodes = configurationServer.CatalogNode.QueryChildren(
//                new[] { CatalogResourceTypes.ProjectCollection },
//                false, CatalogQueryOptions.None);
//
//            // List the team project collections
//            foreach (CatalogNode collectionNode in collectionNodes)
//            {
//                // Use the InstanceId property to get the team project collection
//                Guid collectionId = new Guid(collectionNode.Resource.Properties["InstanceId"]);
//                TfsTeamProjectCollection teamProjectCollection = configurationServer.GetTeamProjectCollection(collectionId);
//
//                // Print the name of the team project collection
//                Console.WriteLine("Collection: " + teamProjectCollection.Name);
//
//                // Get a catalog of team projects for the collection
//                ReadOnlyCollection<CatalogNode> projectNodes = collectionNode.QueryChildren(
//                    new[] { CatalogResourceTypes.TeamProject },
//                    false, CatalogQueryOptions.None);
//
//                // List the team projects in the collection
//                foreach (CatalogNode projectNode in projectNodes)
//                {
//                    Console.WriteLine(" Team Project: " + projectNode.Resource.DisplayName);
//                }
//            }
//        }
    
    }
}
