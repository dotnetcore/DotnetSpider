using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;

namespace DotnetSpider.Extension.Infrastructure
{
	public class SelectorUtils
	{
		public static ISelector Parse(BaseSelector selector)
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
							if (string.IsNullOrEmpty(selector.Argument))
							{
								return Selectors.Regex(expression);
							}
							else
							{
								if (int.TryParse(selector.Argument, out var group))
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
							throw new SpiderException($"Selector {selector} unsupoort.");
						}
				}
			}
			else
			{
				throw new SpiderException("Selector shold not be null");
			}
		}

		public static void NotNullExpression(BaseSelector selector)
		{
			if (string.IsNullOrEmpty(selector.Expression))
			{
				throw new SpiderException($"Expression of {selector} should not be null/empty.");
			}
		}

		public static ISelector Parse(Selector selector)
		{
			if (!string.IsNullOrEmpty(selector?.Expression))
			{
				string expression = selector.Expression;

				switch (selector.Type)
				{
					case SelectorType.Css:
						{
							return Selectors.Css(expression);
						}
					case SelectorType.Enviroment:
						{
							return Selectors.Enviroment(expression);
						}
					case SelectorType.JsonPath:
						{
							return Selectors.JsonPath(expression);
						}
					case SelectorType.Regex:
						{
							return Selectors.Regex(expression);
						}
					case SelectorType.XPath:
						{
							return Selectors.XPath(expression);
						}
				}
			}

			throw new SpiderException("Not support selector: " + selector);
		}
	}
}
