using System.Collections.Generic;

namespace DotnetSpider.Infrastructure
{
    public class PagedQueryResult<T>
    {
        public int Count { get; private set; }
        public int Page { get; private set; }
        public int Limit { get; private set; }
        public IEnumerable<T> Data { get; private set; }

        public PagedQueryResult(int page, int limit, int count, IEnumerable<T> data)
        {
            Page = page;
            Limit = limit;
            Count = count;
            Data = data;
        }
    }
}