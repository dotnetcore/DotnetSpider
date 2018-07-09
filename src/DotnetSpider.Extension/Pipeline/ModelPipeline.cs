using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Pipeline;
using System;
using System.Linq;

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
		/// <param name="spider">爬虫</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		protected abstract int Process(IModel model, IEnumerable<dynamic> datas, ISpider spider);

		/// <summary>
		/// 处理页面解析器解析到的数据结果
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="spider">爬虫</param>
		public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
		{
			if (resultItems == null)
			{
				return;
			}

			foreach (var resultItem in resultItems)
			{
				resultItem.Request.CountOfResults = 0;
				resultItem.Request.EffectedRows = 0;

				foreach (var kv in resultItem.Results)
				{
					var value = kv.Value as Tuple<IModel, IEnumerable<dynamic>>;

					if (value?.Item2 != null && value.Item2.Any())
					{
						resultItem.Request.CountOfResults += value.Item2.Count();
						resultItem.Request.EffectedRows += Process(value.Item1, value.Item2, spider);
					}
				}
			}
		}
	}
}