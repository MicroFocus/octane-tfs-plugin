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
        public TfsManager()
        {
            MockJobList.Jobs.Add(new PipelineNode("e7c63454-f81d-4786-8a33-cc1e7c9fa5ce", "Tfs Test Job"));
            MockJobList.Jobs.Add(new PipelineNode("f9e21042-a286-44bf-808a-8c9462cb3666", "Tfs Test Job 2"));
            MockJobList.Jobs.Add(new PipelineNode("d45f798c-ccf7-48dc-970d-3b320d63c75c", "Tfs Test Job 3"));        
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
