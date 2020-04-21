using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 实体存储器
	/// </summary>
	public abstract class EntityStorageBase : AbstractDataFlow
	{
		private readonly Type _baseType = typeof(IEntity);
		protected abstract Task StoreAsync(DataContext context, Dictionary<Type, List<dynamic>> dict);

		public override async Task HandleAsync(DataContext context)
		{
			if (context.IsEmpty)
			{
				Logger.LogWarning("数据流上下文不包含实体解析结果");
				return;
			}

			var data = context.GetData();
			var dict = new Dictionary<Type, List<dynamic>>();
			foreach (var d in data)
			{
				var type = d.Key as Type;
				if (type == null || !_baseType.IsAssignableFrom(type))
				{
					continue;
				}

				if (d.Value is IEnumerable list)
				{
					foreach (var obj in list)
					{
						InsertData(dict, type, obj);
					}
				}
				else
				{
					InsertData(dict, type, d.Value);
				}
			}

			await StoreAsync(context, dict);
		}

		private void InsertData(Dictionary<Type, List<object>> dict, Type type, dynamic obj)
		{
			if (_baseType.IsInstanceOfType(obj))
			{
				if (!dict.ContainsKey(type))
				{
					dict.Add(type, new List<dynamic>());
				}

				dict[type].Add(obj);
			}
		}
	}
}
