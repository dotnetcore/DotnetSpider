using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Pipeline;
using System.Collections.Concurrent;
using System;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityPipeline : BasePipeline, IEntityPipeline
	{
		public abstract int Process(string name, List<dynamic> datas);

		public override void Process(params ResultItems[] resultItems)
		{
			if (resultItems == null || resultItems.Length == 0)
			{
				return;
			}

			foreach (var resultItem in resultItems)
			{
				int count = 0;
				int effectedRow = 0;
				foreach (var result in resultItem.Results)
				{
					List<dynamic> list = new List<dynamic>();
					dynamic data = resultItem.GetResultItem(result.Key);
					var t = data.GetType();
					if (data is ISpiderEntity)
					{
						list.Add(data);
					}
					else
					{
						list.AddRange(data);
					}
					if (list.Count > 0)
					{
						count += list.Count;
						effectedRow += Process(result.Key, list);
					}
				}
				resultItem.AddOrUpdateResultItem(ResultItems.CountOfResultsKey, count);
				resultItem.AddOrUpdateResultItem(ResultItems.EffectedRows, effectedRow);
			}
		}

		internal abstract void AddEntity(IEntityDefine type);
	}
}
