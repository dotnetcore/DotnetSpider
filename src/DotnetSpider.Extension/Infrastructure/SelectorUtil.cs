using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;

namespace DotnetSpider.Extension.Infrastructure
{
	public class SelectorUtil
	{
		public static ISelector Parse(BaseSelector selector)
		{
			if (string.IsNullOrEmpty(selector?.Expression))
			{
				return null;
			}

			string expression = selector.Expression;

			switch (selector.Type)
			{
				case SelectorType.Css:
					{
						return Selectors.Css(expression);
					}
				case SelectorType.Enviroment:
					{
						return Selectors.Enviroment (expression);
					}
				case SelectorType.JsonPath:
					{
						return Selectors.JsonPath(expression);
					}
				case SelectorType.Regex:
					{
						if (string.IsNullOrEmpty(selector.Argument))
						{
							return Selectors.Regex(expression);
						}
						else
						{
							int group;
							if (int.TryParse(selector.Argument, out group))
							{
								return Selectors.Regex(expression, group);
							}
							throw new SpiderException("Regex argument should be a number set to group: " + selector);
						}
					}
				case SelectorType.XPath:
					{
						return Selectors.XPath(expression);
					}
			}
			throw new SpiderException("Not support selector: " + selector);
		}

		public static ISelector Parse(Selector selector)
		{
			if (string.IsNullOrEmpty(selector?.Expression))
			{
				return null;
			}

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
			throw new SpiderException("Not support selector: " + selector);
		}
	}
}
