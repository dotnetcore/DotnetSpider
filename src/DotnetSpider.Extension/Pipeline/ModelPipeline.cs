using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using System;
using System.Linq;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Common;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 爬虫模型对应的数据管道
	/// </summary>
	public abstract class ModelPipeline : BasePipeline
	{
		/// <summary>
		/// 处理爬虫实体解析器解析到的实体数据结果
		/// </summary>
		/// <param name="model">数据模型</param>
		/// <param name="datas">数据</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		protected abstract int Process(IModel model, IList<dynamic> datas, ILogger logger, dynamic sender = null);

		/// <summary>
		/// 处理页面解析器解析到的数据结果
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		public override void Process(IList<ResultItems> resultItems, ILogger logger, dynamic sender = null)
		{
			if (resultItems == null)
			{
				return;
			}

			foreach (var resultItem in resultItems)
			{
				foreach (var kv in resultItem.Results)
				{
					var value = kv.Value as Tuple<IModel, IList<dynamic>>;

					if (value?.Item2 != null && value.Item2.Any())
					{
						resultItem.Request.AddCountOfResults(value.Item2.Count);
						int effectedRows = Process(value.Item1, value.Item2, logger, sender);
						resultItem.Request.AddEffectedRows(effectedRows);
					}
				}
			}
		}
	}
}