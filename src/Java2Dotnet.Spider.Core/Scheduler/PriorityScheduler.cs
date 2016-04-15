using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Java2Dotnet.Spider.Common;

namespace Java2Dotnet.Spider.Core.Scheduler
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

		protected override void PushWhenNoDuplicate(Request request, ISpider spider)
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

		public override void ResetDuplicateCheck(ISpider spider)
		{
			_noPriorityQueue.Clear();
			_priorityQueuePlus.Clear();
			_priorityQueueMinus.Clear();
		}

		public override Request Poll(ISpider spider)
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

		public int GetLeftRequestsCount(ISpider spider)
		{
			return _noPriorityQueue.Count;
		}

		public int GetTotalRequestsCount(ISpider spider)
		{
			return DuplicateRemover.GetTotalRequestsCount(spider);
		}

		public override void Load(HashSet<Request> requests, ISpider spider)
		{
			throw new NotImplementedException();
		}

		public override HashSet<Request> ToList(ISpider spider)
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

