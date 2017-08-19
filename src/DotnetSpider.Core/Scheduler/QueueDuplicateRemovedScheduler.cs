using DotnetSpider.Core.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Basic Scheduler implementation. 
	/// </summary>
	public sealed class QueueDuplicateRemovedScheduler : DuplicateRemovedScheduler
	{
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private List<Request> _queue = new List<Request>();
		private readonly AutomicLong _successCounter = new AutomicLong(0);
		private readonly AutomicLong _errorCounter = new AutomicLong(0);

		public override bool IsNetworkScheduler => false;

		protected override void PushWhenNoDuplicate(Request request)
		{
			_lock.EnterWriteLock();
			try
			{
				_queue.Add(request);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		public override void ResetDuplicateCheck()
		{
			_lock.EnterWriteLock();
			try
			{
				_queue.Clear();
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		public override Request Poll()
		{
			_lock.EnterWriteLock();
			try
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
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		public override long GetLeftRequestsCount()
		{
			_lock.EnterWriteLock();
			try
			{
				return _queue.Count;
			}
			finally
			{
				_lock.ExitWriteLock();
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
			_lock.EnterWriteLock();
			try
			{
				_queue = new List<Request>(requests);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		public override HashSet<Request> ToList()
		{
			_lock.EnterWriteLock();
			try
			{
				return new HashSet<Request>(_queue.ToArray());
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		public override void Dispose()
		{
			_lock.EnterWriteLock();
			try
			{
				_queue.Clear();
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		
			base.Dispose();
		}
	}
}