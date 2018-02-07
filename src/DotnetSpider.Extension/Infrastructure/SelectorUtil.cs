using Cassandra;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using System;
using System.Diagnostics.Contracts;

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// Selector的帮助类
	/// </summary>
	public static class SelectorUtil
	{
		/// <summary>
		/// 取得页面信息中的环境变量值
		/// </summary>
		/// <param name="field">环境变量名称</param>
		/// <param name="page">页面信息</param>
		/// <param name="index">当前属性在所有属性中的索引</param>
		/// <returns>环境变量值</returns>
		public static object GetEnviromentValue(string field, Page page, int index)
		{
			var key = field.ToLower();
			switch (key)
			{
				case "timeuuid":
					{
						return TimeUuid.NewId();
					}
				case "url":
					{
						return page.Url;
					}
				case "targeturl":
					{
						return page.TargetUrl;
					}
				case "now":
					{
						return DateTime.Now;
					}
				case "monday":
					{
						return DateTimeUtil.Monday;
					}
				case "today":
					{
						return DateTime.Now.Date;
					}
				case "monthly":
					{
						return DateTimeUtil.FirstDayOfTheMonth;
					}
				case "index":
					{
						return index;
					}
				default:
					{
						var v1 = page.Request.GetExtra(field);
						if (v1 == null)
						{
							var v2 = page.Request.GetExtra(key);
							return v2;
						}
						return v1;
					}
			}
		}

		/// <summary>
		/// 把BaseSelector转换成真正的查询器
		/// </summary>
		/// <param name="selector">BaseSelector</param>
		/// <returns>查询器</returns>
		public static ISelector ToSelector(this ISelectorAttribute selector)
		{
			if (selector != null)
			{
				string expression = selector.Expression;

				switch (selector.Type)
				{
					case SelectorType.Css:
						{
							NotNullExpression(selector);
							return Selectors.Css(expression);
						}
					case SelectorType.Enviroment:
						{
							return Selectors.Enviroment(expression);
						}
					case SelectorType.JsonPath:
						{
							NotNullExpression(selector);
							return Selectors.JsonPath(expression);
						}
					case SelectorType.Regex:
						{
							NotNullExpression(selector);
							if (string.IsNullOrEmpty(selector.Arguments))
							{
								return Selectors.Regex(expression);
							}
							else
							{
								if (int.TryParse(selector.Arguments, out var group))
								{
									return Selectors.Regex(expression, group);
								}
								throw new SpiderException("Regex argument should be a number set to group: " + selector);
							}
						}
					case SelectorType.XPath:
						{
							NotNullExpression(selector);
							return Selectors.XPath(expression);
						}
					default:
						{
							throw new SpiderException($"Selector {selector} unsupoort");
						}
				}
			}
			else
			{
				return null;
			}
		}

		internal static void NotNullExpression(ISelectorAttribute selector)
		{
			if (string.IsNullOrWhiteSpace(selector.Expression))
			{
				throw new ArgumentException($"Expression of {selector} should not be null/empty.");
			}
		}
	}
}
