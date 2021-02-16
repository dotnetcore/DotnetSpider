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
	public abstract class EntityStorageBase : DataFlowBase
	{
		private readonly Type _baseType = typeof(IEntity);

		/// <summary>
		///
		/// </summary>
		/// <param name="context">数据流上下文</param>
		/// <param name="entities">数据解析结果 (数据类型, List(数据对象))</param>
		/// <returns></returns>
		protected abstract Task HandleAsync(DataFlowContext context, IDictionary<Type, ICollection<dynamic>> entities);

		public override async Task HandleAsync(DataFlowContext context)
		{
			if (IsNullOrEmpty(context))
			{
				Logger.LogWarning("数据流上下文不包含实体解析结果");
				return;
			}

			var data = context.GetData();
			var result = new Dictionary<Type, ICollection<dynamic>>();
			foreach (var kv in data)
			{
				var type = kv.Key as Type;
				if (type == null || !_baseType.IsAssignableFrom(type))
				{
					continue;
				}

				if (kv.Value is IEnumerable list)
				{
					foreach (var obj in list)
					{
						AddResult(result, type, obj);
					}
				}
				else
				{
					AddResult(result, type, kv.Value);
				}
			}

			await HandleAsync(context, result);
		}

		private void AddResult(IDictionary<Type, ICollection<dynamic>> dict, Type type, dynamic obj)
		{
			if (!_baseType.IsInstanceOfType(obj))
			{
				return;
			}

			if (!dict.ContainsKey(type))
			{
				dict.Add(type, new List<dynamic>());
			}

			dict[type].Add(obj);
		}
	}
}
