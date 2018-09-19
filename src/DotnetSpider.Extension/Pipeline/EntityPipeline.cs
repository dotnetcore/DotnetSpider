using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using System;
using System.Linq;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 爬虫模型对应的数据管道
	/// </summary>
	public abstract class EntityPipeline : BasePipeline
	{
		/// <summary>
		/// 处理爬虫实体解析器解析到的实体数据结果
		/// </summary>
		/// <param name="datas">数据</param>
		/// <param name="sender">调用方</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		protected abstract int Process(IEnumerable<IBaseEntity> datas, dynamic sender = null);

		/// <summary>
		/// 处理页面解析器解析到的数据结果
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		public override void Process(IList<ResultItems> resultItems, dynamic sender = null)
		{
			if (resultItems == null)
			{
				return;
			}

			foreach (var resultItem in resultItems)
			{
				// 用于清洗不同类型的数据
				var typeMapEntities = new Dictionary<string, List<IBaseEntity>>();

				foreach (var kv1 in resultItem)
				{
					var entities = kv1.Value as IEnumerable<dynamic>;
					if (entities != null)
					{
						foreach (var entity in entities)
						{
							if (entity is IBaseEntity)
							{
								var type = entity.GetType().FullName;
								if (!typeMapEntities.ContainsKey(type))
								{
									typeMapEntities.Add(type, new List<IBaseEntity>());
								}
								typeMapEntities[type].Add(entity);
							}
						}
					}
					else
					{
						var singleEntity = kv1.Value as IBaseEntity;
						if (singleEntity != null)
						{
							var type = singleEntity.GetType().FullName;
							typeMapEntities.Add(type, new List<IBaseEntity> { singleEntity });
						}
					}

					foreach (var kv2 in typeMapEntities)
					{
						resultItem.Request.AddCountOfResults(kv2.Value.Count);
						int effectedRows = Process(kv2.Value, sender);
						resultItem.Request.AddEffectedRows(effectedRows);
					}
				}
			}

		}
	}
}