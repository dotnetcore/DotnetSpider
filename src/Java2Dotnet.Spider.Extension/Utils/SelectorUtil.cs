using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Selector;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Model;

namespace Java2Dotnet.Spider.Extension.Utils
{
	public class SelectorUtil
	{
		public static ISelector GetSelector(Selector selector)
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
						return new CssHtmlSelector(expression);
					}
				case ExtractType.Enviroment:
					{
						return new EnviromentSelector(expression);
					}
				case ExtractType.JsonPath:
					{
						return new JsonPathSelector(expression);
					}
				case ExtractType.Regex:
					{
						if (string.IsNullOrEmpty(selector.Argument?.ToString()))
						{
							return new RegexSelector(expression);
						}
						else
						{
							int group;
							if (int.TryParse(selector.Argument.ToString(), out group))
							{
								return new RegexSelector(expression, group);
							}
							throw new SpiderExceptoin("Regex argument shoulb be a number set to group: " + selector);
						}
					}
				case ExtractType.XPath:
					{
						return new XPathSelector(expression);
					}
			}
			throw new SpiderExceptoin("Not support selector: " + selector);
		}
	}
}
