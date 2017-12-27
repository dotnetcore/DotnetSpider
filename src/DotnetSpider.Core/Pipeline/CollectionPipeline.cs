using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	public class CollectionPipeline : BasePipeline, ICollectionPipeline
	{
		private readonly  BlockingCollection<ResultItems> _items = new  BlockingCollection<ResultItems>();

		public IEnumerable<ResultItems> GetCollection()
		{
			return _items;
		}

		public override void Process(IEnumerable<ResultItems> resultItems)
		{
			foreach(var item in resultItems)
			{
				_items.Add(item);
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			_items.Dispose();
		}
	}
}
