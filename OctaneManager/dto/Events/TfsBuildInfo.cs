
namespace MicroFocus.Ci.Tfs.Octane.Dto.Events
{
	public class TfsBuildInfo
	{
		public string CollectionName { get; internal set; }
		public string Project { get; internal set; }
		public string BuildDefinitionId { get; internal set; }
		public string BuildId { get; internal set; }
		public string BuildName { get; internal set; }

		public override string ToString()
		{
			return $"{CollectionName}.{Project}.{BuildDefinitionId}.{BuildId}.{BuildName}";
		}
	}
}
