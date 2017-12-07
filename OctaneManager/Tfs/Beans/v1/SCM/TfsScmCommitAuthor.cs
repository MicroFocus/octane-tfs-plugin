using System;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1.SCM
{
	public class TfsScmCommitAuthor
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public DateTime Date { get; set; }

		public override string ToString()
		{
			return Email;
		}
	}
}
