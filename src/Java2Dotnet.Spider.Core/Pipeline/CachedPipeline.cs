using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

#if NET_CORE
using Java2Dotnet.Spider.JLog;
#else
using log4net;
#endif

namespace Java2Dotnet.Spider.Core.Pipeline
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class CachedPipeline : IPipeline
	{
		private readonly ConcurrentDictionary<ISpider, List<ResultItems>> _cached = new ConcurrentDictionary<ISpider, List<ResultItems>>();

#if NET_CORE
		protected static readonly ILog Logger = LogManager.GetLogger();
#else
		protected static readonly ILog Logger = LogManager.GetLogger(typeof(CachedPipeline));
#endif

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
