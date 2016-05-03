using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Java2Dotnet.Spider.JLog;

namespace Java2Dotnet.Spider.Core.Pipeline
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class CachedPipeline : IPipeline
	{
		private readonly ConcurrentDictionary<ISpider, List<ResultItems>> _cached = new ConcurrentDictionary<ISpider, List<ResultItems>>();
		public int CachedSize { get; set; } = 1;

		protected abstract void Process(List<ResultItems> resultItemsList, ISpider spider);

		public void Process(ResultItems resultItems, ISpider spider)
		{
			if (_cached.ContainsKey(spider))
			{
				_cached[spider].Add(resultItems);
			}
			else
			{
				while (!_cached.TryAdd(spider, new List<ResultItems>() { resultItems }))
				{
				}
			}

			if (_cached[spider].Count >= CachedSize)
			{
				List<ResultItems> result = new List<ResultItems>();

				result.AddRange(_cached[spider]);
				_cached.Clear();

				// 做成异步
				Process(result.ToList(), spider);
			}
		}

		public void Dispose()
		{
			foreach (var entry in _cached)
			{
				if (entry.Value.Count > 0)
				{
					Process(entry.Value, entry.Key);
					_cached.Clear();
				}
			}
		}
	}
}
