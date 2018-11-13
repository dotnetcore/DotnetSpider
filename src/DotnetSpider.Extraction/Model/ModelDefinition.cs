using DotnetSpider.Extraction.Model.Attribute;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotnetSpider.Extraction.Model
{
	public class ModelDefinition : IModel
	{
		/// <summary>
		/// 数据模型的选择器
		/// </summary>
		public Selector Selector { get; protected set; }

		/// <summary>
		/// 从最终解析到的结果中取前 Take 个实体
		/// </summary>
		public int Take { get; protected set; }

		/// <summary>
		/// 设置 Take 的方向, 默认是从头部取
		/// </summary>
		public bool TakeFromHead { get; protected set; }

		/// <summary>
		/// 爬虫实体定义的数据库列信息
		/// </summary>
		public HashSet<Field> Fields { get; protected set; }

		/// <summary>
		/// 目标链接的选择器
		/// </summary>
		public IEnumerable<Target> Targets { get; protected set; }

		/// <summary>
		/// 共享值的选择器
		/// </summary>
		public IEnumerable<Share> Shares { get; protected set; }

		[JsonIgnore]
		public string Identity { get; protected set; }

		public ModelDefinition(Selector selector, IEnumerable<Field> fields,
			Target targetRequestSelector)
			: this(selector, fields, new[] { targetRequestSelector })
		{
		}

		public ModelDefinition(Selector selector, IEnumerable<Field> fields,
			IEnumerable<Target> targets = null,
			IEnumerable<Share> sharedValueSelectors = null, int take = 0, bool takeFromHead = true)
		{
			Selector = selector;
			if (fields == null)
			{
				throw new ExtractionException($"{nameof(fields)} should not be null.");
			}

			Fields = new HashSet<Field>(fields);
			if (Fields.Count == 0)
			{
				throw new ExtractionException("Count of fields should large than 0.");
			}

			Targets = targets;
			Shares = sharedValueSelectors;
			Take = take;
			TakeFromHead = takeFromHead;
			Identity = Guid.NewGuid().ToString("N");
		}

		public ModelDefinition(Type type)
		{
			var entitySelector = type.GetCustomAttributes(typeof(Entity), true).FirstOrDefault() as Entity;
			int take = 0;
			bool takeFromHead = true;
			Selector selector = null;
			if (entitySelector != null)
			{
				take = entitySelector.Take;
				takeFromHead = entitySelector.TakeFromHead;
				selector = new Selector { Expression = entitySelector.Expression, Type = entitySelector.Type };
			}

			var targets = type.GetCustomAttributes(typeof(Target), true).Select(s => (Target)s).ToList();
			var sharedValueSelectors = type.GetCustomAttributes(typeof(Share), true).Select(e =>
			{
				var p = (Share)e;
				return new Share
				{
					Name = p.Name,
					Expression = p.Expression,
					Type = p.Type
				};
			}).ToList();

			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			var fields = new HashSet<Field>();
			foreach (var property in properties)
			{
				var field = property.GetCustomAttributes(typeof(Field), true).FirstOrDefault() as Field;

				if (field == null)
				{
					continue;
				}

				field.Name = property.Name;
				field.Formatters = property.GetCustomAttributes(typeof(Formatter.Formatter), true).Select(p => (Formatter.Formatter)p).ToArray();
				fields.Add(field);
			}

			Selector = selector;

			Fields = fields;
			Targets = targets;
			Shares = sharedValueSelectors;
			Take = take;
			TakeFromHead = takeFromHead;

			Identity = type.FullName;
		}
	}

	public class ModelDefinition<T> : ModelDefinition where T : IBaseEntity
	{
		public ModelDefinition() : base(typeof(T))
		{
		}
	}
}