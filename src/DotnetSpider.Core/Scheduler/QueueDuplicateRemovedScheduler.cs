using DotnetSpider.Core.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Basic Scheduler implementation. 
	/// Store urls to fetch in LinkedBlockingQueue and remove duplicate urls by HashMap.
	/// </summary>
	public sealed class QueueDuplicateRemovedScheduler : DuplicateRemovedScheduler
	{
		private List<Request> _queue = new List<Request>();
		private readonly AutomicLong _successCounter = new AutomicLong(0);
		private readonly AutomicLong _errorCounter = new AutomicLong(0);

		protected override void PushWhenNoDuplicate(Request request)
		{
			// ReSharper disable once InconsistentlySynchronizedField
			_queue.Add(request);
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
				if (_queue.Count == 0)
				{
					return null;
				}
				else
				{
					Request request;
					if (DepthFirst)
					{
						request = _queue.Last();
						_queue.RemoveAt(_queue.Count - 1);
					}
					else
					{
						request = _queue.First();
						_queue.RemoveAt(0);
					}

					return request;
				}
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

		public override void Import(HashSet<Request> requests)
		{
			lock (this)
			{
				_queue = new List<Request>(requests);
			}
		}

		public override HashSet<Request> ToList()
		{
			lock (this)
			{
				return new HashSet<Request>(_queue.ToArray());
			}
		}

		public override void Dispose()
		{
			_queue.Clear();
			base.Dispose();
		}
	}
}