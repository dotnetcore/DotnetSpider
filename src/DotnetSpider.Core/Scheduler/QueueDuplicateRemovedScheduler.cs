using DotnetSpider.Core.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Basic Scheduler implementation. 
	/// </summary>
	public sealed class QueueDuplicateRemovedScheduler : DuplicateRemovedScheduler
	{
		private readonly object _lock = new object();
		private List<Request> _queue = new List<Request>();
		private readonly AutomicLong _successCounter = new AutomicLong(0);
		private readonly AutomicLong _errorCounter = new AutomicLong(0);

		public override bool IsNetworkScheduler => false;

		protected override void PushWhenNoDuplicate(Request request)
		{
			lock (_lock)
			{
				_queue.Add(request);
			}
		}

		public override void ResetDuplicateCheck()
		{
			lock (_lock)
			{
				_queue.Clear();
			}
		}

		public override Request Poll()
		{
			lock (_lock)
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

		public override long LeftRequestsCount
		{
			get
			{
				lock (_lock)
				{
					return _queue.Count;
				}
			}
		}

		public override long TotalRequestsCount => DuplicateRemover.TotalRequestsCount;

		public override long SuccessRequestsCount => _successCounter.Value;

		public override long ErrorRequestsCount => _errorCounter.Value;

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
			lock (_lock)
			{
				_queue = new List<Request>(requests);
			}
		}

		public override HashSet<Request> ToList()
		{
			lock (_lock)
			{
				return new HashSet<Request>(_queue.ToArray());
			}
		}

		public override void Dispose()
		{
			lock (_lock)
			{
				_queue.Clear();
			}

			base.Dispose();
		}
	}
}