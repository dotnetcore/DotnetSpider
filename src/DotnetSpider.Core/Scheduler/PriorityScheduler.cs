using System;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Priority scheduler. Request with higher priority will poll earlier.
	/// </summary>
	public class PriorityScheduler : DuplicateRemovedScheduler
	{
		public static int InitialCapacity = 5;

		private readonly Queue<Request> _noPriorityQueue = new Queue<Request>();
		private readonly PriorityBlockingQueue<Request> _priorityQueuePlus = new PriorityBlockingQueue<Request>(InitialCapacity);
		private readonly PriorityBlockingQueue<Request> _priorityQueueMinus = new PriorityBlockingQueue<Request>(InitialCapacity, new Comparator());
		private readonly AutomicLong _successCounter = new AutomicLong(0);
		private readonly AutomicLong _errorCounter = new AutomicLong(0);

		protected override bool UseInternet { get; set; } = false;

		public override long LeftRequestsCount => _noPriorityQueue.Count;

		public override long TotalRequestsCount => DuplicateRemover.TotalRequestsCount;

		public override long SuccessRequestsCount => _successCounter.Value;

		public override long ErrorRequestsCount => _errorCounter.Value;

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

		public override void IncreaseSuccessCount()
		{
			_successCounter.Inc();
		}

		public override void IncreaseErrorCount()
		{
			_errorCounter.Inc();
		}

		public override void Import(HashSet<Request> requests)
		{
			throw new NotImplementedException();
		}

		public virtual HashSet<Request> ToList()
		{
			throw new NotImplementedException();
		}

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

		private class Comparator : IComparer<Request>
		{
			public int Compare(Request x, Request y)
			{
				if (x == null || y == null)
				{
					return -1;
				}
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

