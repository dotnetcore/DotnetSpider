using System.Collections.Generic;

namespace DotnetSpider.Infrastructure
{
	public class PagedResult<TEntity>
	{
		public IEnumerable<TEntity> Data { get; }

		public int Count { get; }

		public int Page { get; }

		public int Limit { get; }

		public PagedResult(int page, int limit, int count, IEnumerable<TEntity> data)
		{
			Page = page;
			Limit = limit;
			Count = count;
			Data = data;
		}
	}
}
