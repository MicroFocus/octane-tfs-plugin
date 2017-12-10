using System.Collections.Generic;
using MicroFocus.Ci.Tfs.Octane.dto;
using Newtonsoft.Json;

namespace MicroFocus.Ci.Tfs.Octane.Dto.Events
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

