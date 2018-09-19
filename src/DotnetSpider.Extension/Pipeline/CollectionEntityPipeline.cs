using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Extraction.Model;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 内存数据管道, 把所有数据结果存到内存列表中
	/// </summary>
	public class CollectionEntityPipeline : EntityPipeline, ICollectionEntityPipeline
	{
		private readonly Dictionary<string, List<IBaseEntity>> _collector = new Dictionary<string, List<IBaseEntity>>();
		private readonly object _locker = new object();

		/// <summary>
		/// 取得实体名称的所有数据
		/// </summary>
		/// <param name="modeIdentity">爬虫实体名称</param>
		/// <returns>实体数据</returns>
		public IList<IBaseEntity> GetCollection(string modeIdentity)
		{
			lock (_locker)
			{
				if (_collector.TryGetValue(modeIdentity, out var result))
				{
					return result;
				}
			}

			return null;
		}

		/// <summary>
		/// 存储页面解析器解析到的数据结果到内存中
		/// </summary>
		/// <param name="model">数据模型</param>
		/// <param name="datas">数据</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		protected override int Process(IEnumerable<IBaseEntity> datas, dynamic sender = null)
		{
			if (datas == null|| datas.Count() == 0)
			{
				return 0;
			}

			lock (_locker)
			{
				var identity = datas.First().GetType().FullName;
				if (_collector.ContainsKey(identity))
				{
					var list = _collector[identity];
					list.AddRange(datas);
				}
				else
				{
					var list = new List<IBaseEntity>();
					list.AddRange(datas);
					_collector.Add(identity, list);
				}

				return datas.Count();
			}
		}
	}
}