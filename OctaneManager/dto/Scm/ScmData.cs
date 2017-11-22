using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane.dto;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Scm
{
    internal class ScmData : IDtoBase
    {
        [JsonProperty("repository")]
        public ScmRepository Repository { get; set; }

        [JsonProperty("buildRevId")]
        public string BuildRevId { get; set; }

        [JsonProperty("commits")]
        public List<ScmCommit> Commits { get; set; }
    }
}
