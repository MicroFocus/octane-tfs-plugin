using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.General
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
            return TestResultUtils.GenerateOctaneJobCiId(CollectionName, ProjectId, BuildDefId);
        }
        
    }
}
