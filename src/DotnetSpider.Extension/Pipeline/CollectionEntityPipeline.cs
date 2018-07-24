using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Common;
using DotnetSpider.Extraction.Model;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 内存数据管道, 把所有数据结果存到内存列表中
	/// </summary>
	public class CollectionEntityPipeline : ModelPipeline, ICollectionEntityPipeline
	{
		private readonly Dictionary<string, List<dynamic>> _collector = new Dictionary<string, List<dynamic>>();
		private readonly object _locker = new object();

		/// <summary>
		/// 取得实体名称的所有数据
		/// </summary>
		/// <param name="modeIdentity">爬虫实体名称</param>
		/// <returns>实体数据</returns>
		public IList<dynamic> GetCollection(string modeIdentity)
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
		protected override int Process(IModel model, IList<dynamic> datas, ILogger logger, dynamic sender = null)
		{
			if (datas == null|| datas.Count == 0)
			{
				return 0;
			}

			lock (_locker)
			{
				if (_collector.ContainsKey(model.Identity))
				{
					var list = _collector[model.Identity];
					list.AddRange(datas);
				}
				else
				{
					var list = new List<dynamic>();
					list.AddRange(datas);
					_collector.Add(model.Identity, list);
				}

				return datas.Count;
			}
		}
	}
}