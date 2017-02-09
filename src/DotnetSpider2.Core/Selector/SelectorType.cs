using System;

namespace DotnetSpider.Core.Selector
{
	[Flags]
	public enum SelectorType { XPath, Regex, Css, JsonPath, Enviroment }
}
