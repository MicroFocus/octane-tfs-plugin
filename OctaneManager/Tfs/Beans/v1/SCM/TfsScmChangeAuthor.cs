namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1
{
	public class TfsScmChangeAuthor
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public string UniqueName { get; set; }
		public override string ToString()
		{
			return UniqueName;
		}
	}
}
