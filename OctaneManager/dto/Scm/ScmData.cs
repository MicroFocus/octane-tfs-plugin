using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Scm
{
    public class ScmData : IDtoBase
    {
        public ScmRepository Repository { get; set; }
        public string BuildRevId { get; set; }

        public List<ScmCommit> Commits { get; set; }
    }
}
