using MicroFocus.Ci.Tfs.Octane.dto;
using MicroFocus.Ci.Tfs.Octane.dto.general;
using MicroFocus.Ci.Tfs.Octane.dto.pipelines;
using MicroFocus.Ci.Tfs.Octane.Tfs;

namespace MicroFocus.Ci.Tfs.Octane
{
    public class TfsManager : TfsManagerBase
    {
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
                    var buildDefenitions = GetBuildDefenitions(collection, project);
                    foreach (var buildDefenition in buildDefenitions)
                    {
                        result.Jobs.Add(new PipelineNode($"{collection.Id}.{project.Id}.{buildDefenition.Id}",buildDefenition.Name));
                    }
                }
            }
            _cachedJobList = result;

            return _cachedJobList;
        }

        public IDtoBase GetJobDetail(string jobId)
        {
            return _cachedJobList[jobId];
        }     
       
    }
}
