using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.v1
{
	public class TfsLink
	{
		public string Href { get; set; }

		public override string ToString()
		{
			return Href;
		}
	}
}
