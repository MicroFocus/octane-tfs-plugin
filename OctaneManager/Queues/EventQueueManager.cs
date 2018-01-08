using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.RestServer.Queues
{
	public class EventQueueManager
	{
		private static EventQueueManager instance = new EventQueueManager();

		private List<CiEvent> _allEventList = new List<CiEvent>();
		private List<CiEvent> _finishEventList = new List<CiEvent>();
		private readonly CancellationTokenSource _tasksCancellationToken = new CancellationTokenSource();
		private Task _taskEventThread;

		private EventQueueManager()
		{
			_taskEventThread = Task.Factory.StartNew(() => StartEventTask(_tasksCancellationToken.Token), TaskCreationOptions.LongRunning);

		}

		private void StartEventTask(CancellationToken token)
		{
			throw new NotImplementedException();
		}

		public static EventQueueManager GetInstance()
		{
			return instance;
		}

		public void AddEvent(CiEvent ciEvent){
			_allEventList.Add(ciEvent);
			bool isFinishEvent = ciEvent.EventType.Equals(CiEventType.Finished);
			if (isFinishEvent)
			{
				_finishEventList.Add(ciEvent);
			}
		}
	}
}
