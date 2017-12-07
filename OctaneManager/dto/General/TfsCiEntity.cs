using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto.pipelines;

namespace MicroFocus.Ci.Tfs.Octane.Dto.General
{
    public class TfsCiEntity
    {
        public string CollectionName { get; }
        public string ProjectId { get; }
        public string BuildDefId { get; }

        public TfsCiEntity(string collectionName, string projectId, string buildDefId)
        {
            CollectionName = collectionName;
            ProjectId = projectId;
            BuildDefId = buildDefId;
        }

        public override string ToString()
        {
            return PipelineNode.GenerateOctaneJobCiId(CollectionName, ProjectId, BuildDefId);
        }
        
    }
}
