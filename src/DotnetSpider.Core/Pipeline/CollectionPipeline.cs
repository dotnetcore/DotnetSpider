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
		public IEnumerable<ResultItems> GetCollection(dynamic owner)
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
		public override void Process(IEnumerable<ResultItems> resultItems, ILogger logger, dynamic sender)
		{
			lock (ItemsLocker)
			{
				if (!_items.ContainsKey(sender))
				{
					_items.Add(sender, new List<ResultItems>());
				}

				_items[sender].AddRange(resultItems);
			}
		}
	}
}
