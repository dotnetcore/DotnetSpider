using System.Collections.Generic;
using System.Threading;

namespace DotnetSpider.Core.Scheduler.Component
{
	public class HashSetDuplicateRemover : IDuplicateRemover
	{
		private readonly Dictionary<string, string> _urls = new Dictionary<string, string>();
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		public bool IsDuplicate(Request request)
		{
			_lock.EnterWriteLock();
			try
			{
				bool isDuplicate = _urls.ContainsKey(request.Identity);
				if (!isDuplicate)
				{
					_urls.Add(request.Identity, string.Empty);
				}
				return isDuplicate;
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		public void ResetDuplicateCheck()
		{
			_urls.Clear();
		}

		public long GetTotalRequestsCount()
		{
			return _urls.Count;
		}

		public void Dispose()
		{
			_urls.Clear();
		}
	}
}
