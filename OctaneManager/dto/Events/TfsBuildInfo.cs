
namespace MicroFocus.Ci.Tfs.Octane.Dto.Events
{
	public class TfsBuildInfo
	{
		public string CollectionName { get; set; }
		public string Project { get; set; }
		public string BuildDefinitionId { get; set; }
		public string BuildId { get; set; }
		public string BuildName { get; set; }

		public override string ToString()
		{
			return $"{CollectionName}.{Project}.{BuildDefinitionId}.{BuildId}.{BuildName}";
		}
	}
}
