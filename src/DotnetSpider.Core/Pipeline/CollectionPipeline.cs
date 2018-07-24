using DotnetSpider.Common;
using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// 所有数据结果存在内存中
	/// </summary>
	public class CollectionPipeline : BasePipeline, ICollectionPipeline
	{
		private readonly Dictionary<object, List<ResultItems>> _items = new Dictionary<object, List<ResultItems>>();
		private static readonly object ItemsLocker = new object();

		/// <summary>
		/// 获取所有数据结果
		/// </summary>
		/// <param name="owner">数据拥有者</param>
		/// <returns>数据结果</returns>
		public IList<ResultItems> GetCollection(dynamic owner)
		{
			lock (ItemsLocker)
			{
				if (_items.ContainsKey(owner))
				{
					return _items[owner];
				}
				else
				{
					return new ResultItems[0];
				}
			}
		}

		/// <summary>
		/// 处理页面解析器解析到的数据结果
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		public override void Process(IList<ResultItems> resultItems, ILogger logger, dynamic sender = null)
		{
			var identity = GetIdentity(sender);
			lock (ItemsLocker)
			{
				if (!_items.ContainsKey(identity))
				{
					_items.Add(identity, new List<ResultItems>());
				}

				_items[identity].AddRange(resultItems);
			}
		}
	}
}
