
namespace MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems
{
    public class TfsBaseItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
		public override string ToString()
		{
			return $"{Id}-{Name}";
		}
	}
}
