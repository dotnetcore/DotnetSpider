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
	/// <typeparam name="TEntity"></typeparam>
	public class DataParser<TEntity> : DataParser where TEntity : EntityBase<TEntity>, new()
	{
		protected Model<TEntity> Model { get; private set; }

		public override Task InitializeAsync()
		{
			Model = new Model<TEntity>();

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

			return Task.CompletedTask;
		}

		protected virtual TEntity ConfigureDataObject(TEntity t)
		{
			return t;
		}

		protected override Task ParseAsync(DataFlowContext context)
		{
			var selectable = context.Selectable;

			var results = new List<TEntity>();

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

		private TEntity ParseObject(DataFlowContext context, Dictionary<string, object> properties,
			ISelectable selectable,
			int index)
		{
			var dataObject = new TEntity();
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

		private string GetEnvironment(DataFlowContext context, Dictionary<string, object> properties,
			ValueSelector field,
			int index)
		{
			string value;
			switch (field.Expression?.ToUpper())
			{
				case Const.EnvironmentNames.EntityIndex:
				{
					value = index.ToString();
					break;
				}

				case Const.EnvironmentNames.Guid:
				{
					value = Guid.NewGuid().ToString();
					break;
				}

				case Const.EnvironmentNames.Date:
				case Const.EnvironmentNames.Today:
				{
					value = DateTimeOffset.Now.Date.ToString("yyyy-MM-dd");
					break;
				}

				case Const.EnvironmentNames.Datetime:
				case Const.EnvironmentNames.Now:
				{
					value = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss");
					break;
				}

				case Const.EnvironmentNames.Month:
				{
					value = DateTimeHelper.FirstDayOfMonth.ToString("yyyy-MM-dd");
					break;
				}

				case Const.EnvironmentNames.Monday:
				{
					value = DateTimeHelper.Monday.ToString("yyyy-MM-dd");
					break;
				}

				case Const.EnvironmentNames.SpiderId:
				{
					value = context.Request.Owner;
					break;
				}

				case Const.EnvironmentNames.RequestHash:
				{
					value = context.Request.Hash;
					break;
				}
				default:
				{
					value = string.IsNullOrWhiteSpace(field.Expression) ? null :
						properties.ContainsKey(field.Expression) ? properties[field.Expression]?.ToString() : null;
					break;
				}
			}

			return value;
		}
	}
}
