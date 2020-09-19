using System.Collections.Generic;

namespace DotnetSpider.Infrastructure
{
	public class PagedResult<TEntity>
	{
		public IEnumerable<TEntity> Data { get; private set; }

		public int Count { get; private set; }

		public int Page { get; private set; }

		public int Limit { get; private set; }

		public PagedResult(int page, int limit, int count, IEnumerable<TEntity> data)
		{
			Page = page;
			Limit = limit;
			Count = count;
			Data = data;
		}
	}
}
