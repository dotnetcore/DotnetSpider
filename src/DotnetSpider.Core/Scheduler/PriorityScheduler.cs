using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DotnetSpider.Core.Common;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Priority scheduler. Request with higher priority will poll earlier.
	/// </summary>
	public class PriorityScheduler : DuplicateRemovedScheduler, IMonitorableScheduler
	{
		public static int InitialCapacity = 5;
		private readonly Queue<Request> _noPriorityQueue = new Queue<Request>();
		private readonly PriorityBlockingQueue<Request> _priorityQueuePlus = new PriorityBlockingQueue<Request>(InitialCapacity);
		private readonly PriorityBlockingQueue<Request> _priorityQueueMinus = new PriorityBlockingQueue<Request>(InitialCapacity, new Comparator());
		private AutomicLong _successCounter = new AutomicLong(0);
		private AutomicLong _errorCounter = new AutomicLong(0);

		protected override void PushWhenNoDuplicate(Request request)
		{
			if (request.Priority == 0)
			{
				_noPriorityQueue.Enqueue(request);
			}
			else if (request.Priority > 0)
			{
				_priorityQueuePlus.Push(request);
			}
			else
			{
				_priorityQueueMinus.Pop();
			}
		}

		public override void ResetDuplicateCheck()
		{
			_noPriorityQueue.Clear();
			_priorityQueuePlus.Clear();
			_priorityQueueMinus.Clear();
		}

		public override Request Poll()
		{
			lock (this)
			{
				Request poll = _priorityQueuePlus.Pop();
				if (poll != null)
				{
					return poll;
				}
				poll = _noPriorityQueue.Dequeue();
				if (poll != null)
				{
					return poll;
				}
				return _priorityQueueMinus.Pop();
			}
		}

		public long GetLeftRequestsCount()
		{
			return _noPriorityQueue.Count;
		}

		public long GetTotalRequestsCount()
		{
			return DuplicateRemover.GetTotalRequestsCount();
		}

		public long GetSuccessRequestsCount()
		{
			return _successCounter.Value;
		}

		public long GetErrorRequestsCount()
		{
			return _errorCounter.Value;
		}

		public void IncreaseSuccessCounter()
		{
			_successCounter.Inc();
		}

		public void IncreaseErrorCounter()
		{
			_errorCounter.Inc();
		}

		public override void Load(HashSet<Request> requests)
		{
			throw new NotImplementedException();
		}

		public override HashSet<Request> ToList()
		{
			throw new NotImplementedException();
		}

		private class Comparator : IComparer<Request>
		{
			public int Compare(Request x, Request y)
			{
				if (x.Priority > y.Priority)
				{
					return -1;
				}
				if (x.Priority == y.Priority)
				{
					return 0;
				}
				return 1;
			}
		}
	}
}

