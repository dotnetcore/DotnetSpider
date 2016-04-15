using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Java2Dotnet.Spider.Core.Scheduler
{
	/// <summary>
	/// Basic Scheduler implementation. 
	/// Store urls to fetch in LinkedBlockingQueue and remove duplicate urls by HashMap.
	/// </summary>
	public class QueueDuplicateRemovedScheduler : DuplicateRemovedScheduler, IMonitorableScheduler
	{
		private Queue<Request> _queue = new Queue<Request>();

		protected override void PushWhenNoDuplicate(Request request, ISpider spider)
		{
			_queue.Enqueue(request);
		}

		public override void ResetDuplicateCheck(ISpider spider)
		{
			lock (this)
			{
				_queue.Clear();
			}
		}

		public override Request Poll(ISpider spider)
		{
			lock (this)
			{
				return _queue.Count > 0 ? _queue.Dequeue() : null;
			}
		}

		public int GetLeftRequestsCount(ISpider spider)
		{
			lock (this)
			{
				return _queue.Count;
			}
		}

		public int GetTotalRequestsCount(ISpider spider)
		{
			return DuplicateRemover.GetTotalRequestsCount(spider);
		}

		public override void Load(HashSet<Request> requests, ISpider spider)
		{
			lock (this)
			{
				_queue = new Queue<Request>(requests);
			}
		}

		public override HashSet<Request> ToList(ISpider spider)
		{
			lock (this)
			{
				return new HashSet<Request>(_queue.ToArray());
			}
		}
	}
}