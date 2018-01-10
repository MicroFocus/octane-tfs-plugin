using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using System.Collections.Generic;


namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Queue
{
	public class GeneralEventsQueue
	{
		private List<CiEvent> list = new List<CiEvent>();

		public void Add(CiEvent ciEvent)
		{
			list.Add(ciEvent);
		}

		public bool IsEmpty()
		{
			return list.Count == 0;
		}

		public int Count
		{
			get
			{
				return list.Count;
			}
		}

		public IList<CiEvent> GetSnapshot()
		{
			return new List<CiEvent>(list);
		}

		public bool Remove(CiEvent ciEvent)
		{
			return list.Remove(ciEvent);
		}

		public void Clear()
		{
			list.Clear();
		}


	}
}
