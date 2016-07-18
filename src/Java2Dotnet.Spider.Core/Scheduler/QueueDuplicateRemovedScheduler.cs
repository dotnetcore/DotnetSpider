using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Java2Dotnet.Spider.Core.Scheduler
{
	/// <summary>
	/// Basic Scheduler implementation. 
	/// Store urls to fetch in LinkedBlockingQueue and remove duplicate urls by HashMap.
	/// </summary>
	public sealed class QueueDuplicateRemovedScheduler : DuplicateRemovedScheduler, IMonitorableScheduler
	{
		private Queue<Request> _queue = new Queue<Request>();

		protected override void PushWhenNoDuplicate(Request request)
		{
			_queue.Enqueue(request);
		}

		public override void ResetDuplicateCheck()
		{
			lock (this)
			{
				_queue.Clear();
			}
		}

		public override Request Poll()
		{
			lock (this)
			{
				return _queue.Count > 0 ? _queue.Dequeue() : null;
			}
		}

		public int GetLeftRequestsCount()
		{
			lock (this)
			{
				return _queue.Count;
			}
		}

		public int GetTotalRequestsCount()
		{
			return DuplicateRemover.GetTotalRequestsCount();
		}

		public override void Load(HashSet<Request> requests)
		{
			lock (this)
			{
				_queue = new Queue<Request>(requests);
			}
		}

		public override HashSet<Request> ToList()
		{
			lock (this)
			{
				return new HashSet<Request>(_queue.ToArray());
			}
		}
	}
}