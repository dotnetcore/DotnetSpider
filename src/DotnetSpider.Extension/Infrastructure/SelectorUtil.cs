using DotnetSpider.Core;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using System;

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// Selector的帮助类
	/// </summary>
	public static class SelectorUtil
	{
		/// <summary>
		/// 把BaseSelector转换成真正的查询器
		/// </summary>
		/// <param name="selector">BaseSelector</param>
		/// <returns>查询器</returns>
		public static ISelector ToSelector(this Selector selector)
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

		internal static void NotNullExpression(Selector selector)
		{
			if (string.IsNullOrWhiteSpace(selector.Expression))
			{
				throw new ArgumentException($"Expression of {selector} should not be null/empty.");
			}
		}
	}
}
