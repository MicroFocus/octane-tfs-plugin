using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Pipelines;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.General
{
	internal class CiJobList : IDtoBase
	{
		[JsonProperty("jobs")]
		public IList<PipelineNode> Jobs { get; set; }


		public CiJobList()
		{
			Jobs = new List<PipelineNode>();
		}

		public PipelineNode this[string key]
		{
			get { return Jobs.FirstOrDefault(x => x.JobCiId.ToLower() == key.ToLower()); }
			set
			{
				var job = Jobs.FirstOrDefault(x => x.JobCiId == key);
				Jobs[Jobs.IndexOf(job)] = value;
			}
		}
	}
}
