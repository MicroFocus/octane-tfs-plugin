using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.General;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events
{
	public class CiEventsList
	{
		[JsonProperty("server")]
		public CiServerInfo Server { get; set; }

		[JsonProperty("events")]
		public List<CiEvent> Events { get; set; }

		public CiEventsList() => Events = new List<CiEvent>();
	}
}
