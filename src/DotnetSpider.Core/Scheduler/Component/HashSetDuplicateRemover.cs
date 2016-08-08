using System.Collections.Concurrent;

namespace DotnetSpider.Core.Scheduler.Component
{
	public class HashSetDuplicateRemover : IDuplicateRemover
	{
		private readonly ConcurrentDictionary<string, string> _urls = new ConcurrentDictionary<string, string>();

		public bool IsDuplicate(Request request)
		{
			bool isDuplicate = _urls.ContainsKey(request.Identity);
			if (!isDuplicate)
			{
				_urls.GetOrAdd(request.Identity, string.Empty);
			}
			return isDuplicate;
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
