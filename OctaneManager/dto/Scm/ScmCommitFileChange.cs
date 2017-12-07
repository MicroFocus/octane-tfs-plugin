using MicroFocus.Ci.Tfs.Octane.dto;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Scm
{
    public  class ScmCommitFileChange : IDtoBase
    {
		public string Type { get; set; }
		public string File { get; set; }
	}
}
