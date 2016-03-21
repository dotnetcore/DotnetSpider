using System.Collections;
using System.Collections.Generic;

namespace Java2Dotnet.Spider.Core.Pipeline
{
	public class ResultItemsCollectorPipeline : ICollectorPipeline
	{
		// memory will not enough if this list is too large?
		private readonly List<ResultItems> _collector = new List<ResultItems>();

		public void Process(ResultItems resultItems, ISpider spider)
		{
			lock (this)
			{
				_collector.Add(resultItems);
			}
		}

		public IEnumerable GetCollected()
		{
			return _collector;
		}

		public void Dispose()
		{
			_collector.Clear();
		}
	}
}
