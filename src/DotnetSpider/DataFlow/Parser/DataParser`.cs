using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DataFlow.Storage.Model;
using DotnetSpider.Selector;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Parser
{
	/// <summary>
	/// 实体解析器
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DataParser<T> : DataParserBase<T> where T : EntityBase<T>, new()
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public DataParser()
		{
			var followXPaths = new HashSet<string>();
			foreach (var followSelector in Model.FollowSelectors)
			{
				foreach (var xPath in followSelector.XPaths)
				{
					followXPaths.Add(xPath);
				}
			}

			var xPaths = followXPaths.ToArray();
			FollowRequestQuerier = BuildFollowRequestQuerier(DataParserHelper.QueryFollowRequestsByXPath(xPaths));
		}

		protected virtual T ConfigureDataObject(T t)
		{
			return t;
		}

		protected override Task<DataFlowResult> Parse(DataFlowContext context)
		{
			var selectable = context.Selectable;

			var results = new ParseResult<T>();
			if (selectable.Properties == null)
			{
				selectable.Properties = new Dictionary<string, object>();
			}

			var environments = new Dictionary<string, string>();
			foreach (var property in context.Response.Request.Properties)
			{
				environments.Add(property.Key, property.Value);
			}

			if (Model.GlobalValueSelectors != null)
			{
				foreach (var selector in Model.GlobalValueSelectors)
				{
					string name = selector.Name;
					if (string.IsNullOrWhiteSpace(name))
					{
						continue;
					}

					var value = selectable.Select(selector.ToSelector()).GetValue();
					if (!environments.ContainsKey(name))
					{
						environments.Add(name, value);
					}
					else
					{
						environments[name] = value;
					}
				}
			}

			bool singleExtractor = Model.Selector == null;
			if (!singleExtractor)
			{
				var selector = Model.Selector.ToSelector();

				var list = selectable.SelectList(selector).Nodes()?.ToList();
				if (list != null)
				{
					if (Model.Take > 0 && list.Count > Model.Take)
					{
						list = Model.TakeFromHead
							? list.Take(Model.Take).ToList()
							: list.Skip(list.Count - Model.Take).ToList();
					}

					for (var i = 0; i < list.Count; ++i)
					{
						var item = list.ElementAt(i);
						var obj = ParseObject(context, environments, item, i);
						if (obj != null)
						{
							results.Add(obj);
						}
						else
						{
							Logger?.LogWarning($"解析到空数据，类型: {Model.TypeName}");
						}
					}
				}
			}
			else
			{
				var obj = ParseObject(context, environments, selectable, 0);
				if (obj != null)
				{
					results.Add(obj);
				}
				else
				{
					Logger?.LogWarning($"解析到空数据，类型: {Model.TypeName}");
				}
			}

			AddParseResult(context, results);

			return base.Parse(context);
		}

		private T ParseObject(DataFlowContext context, Dictionary<string, string> environments, ISelectable selectable,
			int index)
		{
			var dataObject = new T();
			foreach (var field in Model.ValueSelectors)
			{
				string value = null;
				if (field.Type == SelectorType.Enviroment)
				{
					switch (field.Expression)
					{
						case "INDEX":
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
							value = DateTime.Now.Date.ToString("yyyy-MM-dd");
							break;
						}

						case "DATETIME":
						case "NOW":
						{
							value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							break;
						}

						case "MONTH":
						{
							value = DateTimeHelper.MonthString;
							break;
						}

						case "MONDAY":
						{
							value = DateTimeHelper.MondayString;
							break;
						}

						case "ID":
						{
							value = context.Response.Request.OwnerId;
							break;
						}

						case "REQUEST_HASH":
						{
							value = context.Response.Request.Hash;
							break;
						}

						case "RESPONSE":
						{
							value = JsonConvert.SerializeObject(context.Response);
							break;
						}

						default:
						{
							if (environments.ContainsKey(field.Expression))
							{
								value = environments[field.Expression];
							}

							break;
						}
					}
				}
				else
				{
					var selector = field.ToSelector();
					value = field.ValueOption == ValueOption.Count
						? selectable.SelectList(selector).Nodes().Count().ToString()
						: selectable.Select(selector)?.GetValue(field.ValueOption);
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
								Logger?.LogDebug($"数据格式化失败: {e}");
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
	}
}
