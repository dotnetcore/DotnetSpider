using System.Collections.Generic;

namespace DotnetSpider.Infrastructure
{
	public class PagedQueryResult<T>
	{
		public int Count { get; set; }
		public int Page { get; set; }
		public int Limit { get; set; }
		public IEnumerable<T> Data { get; set; }

		public PagedQueryResult()
		{
		}

		public PagedQueryResult(int page, int limit, int count, IEnumerable<T> data)
		{
			Page = page;
			Limit = limit;
			Count = count;
			Data = data;
		}
	}
}
