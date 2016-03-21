using System.Collections.Concurrent;

namespace Java2Dotnet.Spider.Core.Scheduler.Component
{
	public class HashSetDuplicateRemover : IDuplicateRemover
	{
		private readonly ConcurrentDictionary<string, string> _urls = new ConcurrentDictionary<string, string>();

		public bool IsDuplicate(Request request, ISpider spider)
		{
			bool isDuplicate = _urls.ContainsKey(request.Url.ToString());
			if (!isDuplicate)
			{
				_urls.GetOrAdd(request.Url.ToString(), string.Empty);
			}
			return isDuplicate;
		}

		public void ResetDuplicateCheck(ISpider spider)
		{
			_urls.Clear();
		}

		public int GetTotalRequestsCount(ISpider spider)
		{
			return _urls.Count;
		}

		public void Dispose()
		{
			_urls.Clear();
		}
	}
}
