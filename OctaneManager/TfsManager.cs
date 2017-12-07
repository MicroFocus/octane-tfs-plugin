using System.Reflection;
using log4net;
using MicroFocus.Ci.Tfs.Octane.dto;
using MicroFocus.Ci.Tfs.Octane.dto.general;
using MicroFocus.Ci.Tfs.Octane.dto.pipelines;
using MicroFocus.Ci.Tfs.Octane.Dto.Connectivity;
using MicroFocus.Ci.Tfs.Octane.Tfs;

namespace MicroFocus.Ci.Tfs.Octane
{
    public class TfsManager : TfsManagerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly CiJobList MockJobList = new CiJobList();
        private  CiJobList _cachedJobList = new CiJobList();
        public TfsManager(string pat) : base(pat)
        {
            
        }

        public IDtoBase GetJobsList()
        {
            var result = new CiJobList();
            var collections = GetCollections();

            //TODO: make this simplier
            foreach (var collection in collections)
            {
                var projects = GetProjects(collection);
                foreach (var project in projects)
                {
                    var buildDefinitions = GetBuildDefinitions(collection, project);
                    foreach (var buildDefinition in buildDefinitions)
                    {                        
                        var id = PipelineNode.GenerateOctaneJobCiId(collection.Name, project.Id, buildDefinition.Id);
                        Log.Debug($"New job added to list with id: {id}");
                        result.Jobs.Add(new PipelineNode(id,buildDefinition.Name));
                    }
                }
            }
            _cachedJobList = result;

            return _cachedJobList;
        }

        public IDtoBase GetJobDetail(string jobId)
        {            
            var res =_cachedJobList[jobId];
            if (res == null)
            {
                GetJobsList();
                res = _cachedJobList[jobId];
            }

            var tfsCiEntity = PipelineNode.TranslateOctaneJobCiIdToObject(jobId);
            if (!_subscriptionManager.SubscriptionExists(tfsCiEntity.CollectionName, tfsCiEntity.ProjectId))
            {
                _subscriptionManager.AddBuildCompletion(tfsCiEntity.CollectionName,tfsCiEntity.ProjectId);
            }

            return res;
        }     
       
    }
}
