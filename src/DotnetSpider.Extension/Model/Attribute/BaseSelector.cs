using DotnetSpider.Core.Selector;

namespace DotnetSpider.Extension.Model.Attribute
{
	public class BaseSelector : Selector
	{
		public BaseSelector()
		{
		}

		public BaseSelector(string expression) : base(expression)
		{
		}

		public BaseSelector(SelectorType type, string expression) : base(type, expression)
		{
		}

		public string Argument { get; set; }
	}
}
