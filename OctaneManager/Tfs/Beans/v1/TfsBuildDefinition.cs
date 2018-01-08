using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.ApiItems;
using System;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.v1
{
	public class TfsBuildDefinition : TfsBaseItem
	{
		public string Uri { get; set; }

		public string Type { get; set; }

		public DateTime CreatedDate { get; set; }

		public int Revision { get; set; }
	}
}