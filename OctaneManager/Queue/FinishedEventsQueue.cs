using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Queue
{
	public class FinishedEventsQueue
	{
		private Queue<CiEvent> queue = new Queue<CiEvent>();

		public void Add(CiEvent ciEvent)
		{
			queue.Enqueue(ciEvent);
		}

		public bool IsEmpty()
		{
			return queue.Count == 0;
		}

		public int Count
		{
			get
			{
				return queue.Count;
			}
		}

		public CiEvent Dequeue()
		{
			return queue.Dequeue();
		}

		public CiEvent Peek()
		{
			return queue.Peek();
		}

		public void Clear()
		{
			queue.Clear();
		}
	}
}
