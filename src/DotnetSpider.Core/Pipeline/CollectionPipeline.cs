using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	public class CollectionPipeline : BasePipeline, ICollectionPipeline
	{
		private readonly Dictionary<ISpider, List<ResultItems>> _items = new Dictionary<ISpider, List<ResultItems>>();
		private readonly static object ItemsLocker = new object();

		public IEnumerable<ResultItems> GetCollection(ISpider spider)
		{
			lock (ItemsLocker)
			{
				if (_items.ContainsKey(spider))
				{
					return _items[spider];
				}
				else
				{
					return new ResultItems[0];
				}
			}
		}

		public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
		{
			lock (ItemsLocker)
			{
				if (!_items.ContainsKey(spider))
				{
					_items.Add(spider, new List<ResultItems>());
				}
				_items[spider].AddRange(resultItems);
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			_items.Clear();
		}
	}
}
