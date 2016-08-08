using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Configuration;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Utils
{
	public class SelectorUtil
	{
		public static ISelector Parse(Selector selector)
		{
			if (string.IsNullOrEmpty(selector?.Expression))
			{
				return null;
			}

			string expression = selector.Expression;

			switch (selector.Type)
			{
				case ExtractType.Css:
					{
						return Selectors.Css(expression);
					}
				case ExtractType.Enviroment:
					{
						return Selectors.Enviroment (expression);
					}
				case ExtractType.JsonPath:
					{
						return Selectors.JsonPath(expression);
					}
				case ExtractType.Regex:
					{
						if (string.IsNullOrEmpty(selector.Argument?.ToString()))
						{
							return Selectors.Regex(expression);
						}
						else
						{
							int group;
							if (int.TryParse(selector.Argument.ToString(), out group))
							{
								return Selectors.Regex(expression, group);
							}
							throw new SpiderException("Regex argument shoulb be a number set to group: " + selector);
						}
					}
				case ExtractType.XPath:
					{
						return Selectors.XPath(expression);
					}
			}
			throw new SpiderException("Not support selector: " + selector);
		}
	}
}
