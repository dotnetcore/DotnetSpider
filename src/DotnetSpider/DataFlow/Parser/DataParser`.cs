using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Infrastructure;
using DotnetSpider.Selector;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow.Parser
{
	/// <summary>
	/// 实体解析器
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DataParser<T> : DataParser where T : EntityBase<T>, new()
	{
		public override string Name => $"DataParser<{typeof(T).Name}>";

		protected readonly Model<T> Model;

		/// <summary>
		/// 构造方法
		/// </summary>
		public DataParser()
		{
			Model = new Model<T>();

			var patterns = new HashSet<string>();
			if (Model.FollowRequestSelectors != null)
			{
				foreach (var followSelector in Model.FollowRequestSelectors)
				{
					switch (followSelector.SelectorType)
					{
						case SelectorType.Css:
						{
							foreach (var expression in followSelector.Expressions)
							{
								AddFollowRequestQuerier(Selectors.Css(expression));
							}

							break;
						}
						case SelectorType.Regex:
						{
							foreach (var expression in followSelector.Expressions)
							{
								AddFollowRequestQuerier(Selectors.Regex(expression));
							}

							break;
						}
						case SelectorType.XPath:
						{
							foreach (var expression in followSelector.Expressions)
							{
								AddFollowRequestQuerier(Selectors.XPath(expression));
							}

							break;
						}
						case SelectorType.Environment:
						{
							Logger.LogWarning("SelectorType of follow selector is not supported");
							break;
						}
						case SelectorType.JsonPath:
						{
							foreach (var expression in followSelector.Expressions)
							{
								AddFollowRequestQuerier(Selectors.JsonPath(expression));
							}

							break;
						}
					}

					foreach (var pattern in followSelector.Patterns)
					{
						patterns.Add(pattern);
					}
				}
			}

			foreach (var pattern in patterns)
			{
				AddRequiredValidator(request => Regex.IsMatch(request.RequestUri.ToString(), pattern));
			}
		}

		protected virtual T ConfigureDataObject(T t)
		{
			return t;
		}

		protected override Task Parse(DataContext context)
		{
			var selectable = context.Selectable;

			var results = new List<T>();

			// don't change request properties
			var properties = new Dictionary<string, object>();
			foreach (var property in context.Request.Properties)
			{
				properties[property.Key] = property.Value;
			}

			if (Model.GlobalValueSelectors != null)
			{
				foreach (var selector in Model.GlobalValueSelectors)
				{
					var name = selector.Name;
					if (string.IsNullOrWhiteSpace(name))
					{
						continue;
					}

					var value = selectable.Select(selector.ToSelector()).Value;
					properties[name] = value;
				}
			}

			var single = Model.Selector == null;
			if (!single)
			{
				var selector = Model.Selector.ToSelector();

				var allEntities = selectable.SelectList(selector)?.ToList();
				if (allEntities != null)
				{
					var count = allEntities.Count;
					IEnumerable<ISelectable> entities;
					if (Model.Take > 0 && count > Model.Take)
					{
						entities = Model.TakeByDescending
							? allEntities.Take(Model.Take)
							: allEntities.Skip(count - Model.Take);
					}
					else
					{
						entities = allEntities;
					}

					var index = 0;
					foreach (var entity in entities)
					{
						var obj = ParseObject(context, properties, entity, index);
						if (obj != null)
						{
							results.Add(obj);
						}
						else
						{
							Logger.LogWarning($"解析到空数据，类型: {Model.TypeName}");
						}

						index++;
					}
				}
			}
			else
			{
				var obj = ParseObject(context, properties, selectable, 0);
				if (obj != null)
				{
					results.Add(obj);
				}
				else
				{
					Logger.LogWarning($"解析到空数据，类型: {Model.TypeName}");
				}
			}

			if (results.Count > 0)
			{
				AddParsedResult(context, results);
			}

			return Task.CompletedTask;
		}

		private T ParseObject(DataContext context, Dictionary<string, object> properties, ISelectable selectable,
			int index)
		{
			var dataObject = new T();
			foreach (var field in Model.ValueSelectors)
			{
				string value;
				if (field.Type == SelectorType.Environment)
				{
					value = GetEnvironment(context, properties, field, index);
				}
				else
				{
					var selector = field.ToSelector();
					value = selectable.Select(selector)?.Value;
				}

				if (!string.IsNullOrWhiteSpace(value))
				{
					if (field.Formatters != null && field.Formatters.Length > 0)
					{
						foreach (var formatter in field.Formatters)
						{
#if !DEBUG
							value = formatter.Format(value);
#else
                            try
                            {
                                value = formatter.Format(value);
                            }
                            catch (Exception e)
                            {
                                Logger.LogError($"数据格式化失败: {e}");
                            }
#endif
						}
					}
				}


				var newValue = value == null ? null : Convert.ChangeType(value, field.PropertyInfo.PropertyType);
				if (newValue == null && field.NotNull)
				{
					dataObject = null;
					break;
				}

				field.PropertyInfo.SetValue(dataObject, newValue);
			}

			return ConfigureDataObject(dataObject);
		}

		private string GetEnvironment(DataContext context, Dictionary<string, object> properties, ValueSelector field,
			int index)
		{
			string value;
			switch (field.Expression)
			{
				case "ENTITY_INDEX":
				{
					value = index.ToString();
					break;
				}

				case "GUID":
				{
					value = Guid.NewGuid().ToString();
					break;
				}

				case "DATE":
				case "TODAY":
				{
					value = DateTimeOffset.Now.Date.ToString("yyyy-MM-dd");
					break;
				}

				case "DATETIME":
				case "NOW":
				{
					value = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss");
					break;
				}

				case "MONTH":
				{
					value = DateTime2.FirstDayOfMonth.ToString("yyyy-MM-dd");
					break;
				}

				case "MONDAY":
				{
					value = DateTime2.Monday.ToString("yyyy-MM-dd");
					break;
				}

				case "SPIDER_ID":
				{
					value = context.Request.Owner;
					break;
				}

				case "REQUEST_HASH":
				{
					value = context.Request.Hash;
					break;
				}
				default:
				{
					value = properties.ContainsKey(field.Expression) ? properties[field.Expression].ToString() : null;
					break;
				}
			}

			return value;
		}
	}
}
