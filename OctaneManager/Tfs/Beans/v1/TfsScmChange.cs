using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1;
using System;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems
{
	public class TfsScmChange
	{
		public string Id { get; set; }
		public string Message { get; set; }
		public string Type { get; set; }
		public DateTime Timestamp { get; set; }
		public string Location { get; set; }
		public TfsScmChangeAuthor Author { get; set; }

		public override string ToString()
		{
			return $"{Id}-{Message}";
		}
	}
}
