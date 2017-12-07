using MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems;
using System;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1
{
	public class TfsBuildDefinition : TfsBaseItem
	{
		public string Uri { get; set; }

		public string Type { get; set; }

		public DateTime CreatedDate { get; set; }

		public int Revision { get; set; }
	}
}