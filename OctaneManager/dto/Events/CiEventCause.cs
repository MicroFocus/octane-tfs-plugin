using System.Collections.Generic;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.dto;
using Newtonsoft.Json;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events
{
	public class CiEventCause : IDtoBase
	{
		[JsonProperty("type")]
		public CiEventCauseType CauseType { get; set; }
		[JsonProperty("user")]
		public string User { get; set; }
		[JsonProperty("project")]
		public string Project { get; set; }
		[JsonProperty("buildCiId")]
		public string BuildCiId { get; set; }
		[JsonProperty("causes")]
		public List<CiEventCause> Causes { get; set; }
	}

}

