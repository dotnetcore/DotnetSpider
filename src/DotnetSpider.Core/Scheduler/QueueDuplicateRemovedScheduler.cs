using DotnetSpider.Core.Common;
using System.Collections.Generic;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Basic Scheduler implementation. 
	/// Store urls to fetch in LinkedBlockingQueue and remove duplicate urls by HashMap.
	/// </summary>
	public sealed class QueueDuplicateRemovedScheduler : DuplicateRemovedScheduler
	{
		private Queue<Request> _queue = new Queue<Request>();
		private readonly AutomicLong _successCounter = new AutomicLong(0);
		private readonly AutomicLong _errorCounter = new AutomicLong(0);

		protected override void PushWhenNoDuplicate(Request request)
		{
			// ReSharper disable once InconsistentlySynchronizedField
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

		public override long GetLeftRequestsCount()
		{
			lock (this)
			{
				return _queue.Count;
			}
		}

		public override long GetTotalRequestsCount()
		{
			return DuplicateRemover.GetTotalRequestsCount();
		}

		public override long GetSuccessRequestsCount()
		{
			return _successCounter.Value;
		}

		public override long GetErrorRequestsCount()
		{
			return _errorCounter.Value;
		}

		public override void IncreaseSuccessCounter()
		{
			_successCounter.Inc();
		}

		public override void IncreaseErrorCounter()
		{
			_errorCounter.Inc();
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