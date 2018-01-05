using System.Collections.Generic;
using System.Collections.Concurrent;

namespace DotnetSpider.Core.Scheduler.Component
{
	/// <summary>
	/// 通过哈希去重
	/// </summary>
	public class HashSetDuplicateRemover : IDuplicateRemover
	{
		private readonly ConcurrentDictionary<string, string> _urls = new ConcurrentDictionary<string, string>();
		private readonly object _lock = new object();

		public long TotalRequestsCount => _urls.Count;

		public bool IsDuplicate(Request request)
		{
			lock (_lock)
			{
				bool isDuplicate = _urls.ContainsKey(request.Identity);
				if (!isDuplicate)
				{
					_urls.Add(request.Identity, string.Empty);
				}
				return isDuplicate;
			}
		}

		public void ResetDuplicateCheck()
		{
			_urls.Clear();
		}

		public void Dispose()
		{
			_urls.Clear();
		}
	}
}
