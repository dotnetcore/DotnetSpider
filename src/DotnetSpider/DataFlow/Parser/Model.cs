using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotnetSpider.DataFlow.Storage;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Parser
{
	/// <summary>
	/// 实体模型
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Model<T> where T : EntityBase<T>, new()
	{
		/// <summary>
		/// 实体的类型名称
		/// </summary>
		public string TypeName { get; }

		/// <summary>
		/// 数据模型的选择器
		/// </summary>
		public Selector Selector { get; }

		/// <summary>
		/// 从最终解析到的结果中取前 Take 个实体
		/// </summary>
		public int Take { get; }

		/// <summary>
		/// 设置 Take 的方向, 默认是从头部取
		/// </summary>
		public bool TakeByDescending { get; }

		/// <summary>
		/// 爬虫实体定义的数据库列信息
		/// </summary>
		public HashSet<ValueSelector> ValueSelectors { get; }

		/// <summary>
		/// 目标链接的选择器
		/// </summary>
		public HashSet<FollowRequestSelector> FollowRequestSelectors { get; }

		/// <summary>
		/// 共享值的选择器
		/// </summary>
		public HashSet<GlobalValueSelector> GlobalValueSelectors { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		public Model()
		{
			var type = typeof(T);
			TypeName = type.FullName;
			var entitySelector =
				type.GetCustomAttributes(typeof(EntitySelector), true).FirstOrDefault() as EntitySelector;
			var take = 0;
			var takeByDescending = false;
			Selector selector = null;
			if (entitySelector != null)
			{
				take = entitySelector.Take;
				takeByDescending = entitySelector.TakeByDescending;
				selector = new Selector {Expression = entitySelector.Expression, Type = entitySelector.Type};
			}

			var followRequestSelectors = type.GetCustomAttributes(typeof(FollowRequestSelector), true).Select(x => (FollowRequestSelector) x)
				.ToList();
			var sharedValueSelectors = type.GetCustomAttributes(typeof(GlobalValueSelector), true)
				.Select(x => (GlobalValueSelector) x).ToList();

			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			var valueSelectors = new HashSet<ValueSelector>();
			foreach (var property in properties)
			{
				var valueSelector =
					property.GetCustomAttributes(typeof(ValueSelector), true).FirstOrDefault() as ValueSelector;

				if (valueSelector == null)
				{
					continue;
				}

				valueSelector.Formatters = property.GetCustomAttributes(typeof(Formatter), true)
					.Select(p => (Formatter) p).ToArray();
				valueSelector.PropertyInfo = property;
				valueSelector.NotNull = property.GetCustomAttributes(typeof(Required), false).Any();
				valueSelectors.Add(valueSelector);
			}

			Selector = selector;
			ValueSelectors = valueSelectors;
			FollowRequestSelectors = new HashSet<FollowRequestSelector>(followRequestSelectors);
			GlobalValueSelectors = new HashSet<GlobalValueSelector>(sharedValueSelectors);
			foreach (var valueSelector in GlobalValueSelectors)
			{
				if (string.IsNullOrWhiteSpace(valueSelector.Name))
				{
					throw new SpiderException("Name of global value selector should not be null");
				}
			}

			Take = take;
			TakeByDescending = takeByDescending;
		}
	}
}