using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Selector;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Model;

namespace Java2Dotnet.Spider.Extension.Utils
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
							throw new SpiderExceptoin("Regex argument shoulb be a number set to group: " + selector);
						}
					}
				case ExtractType.XPath:
					{
						return Selectors.XPath(expression);
					}
			}
			throw new SpiderExceptoin("Not support selector: " + selector);
		}
	}
}
