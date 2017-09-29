using DotnetSpider.Core.Selector;

namespace DotnetSpider.Extension.Model
{
	public class Selector : System.Attribute
	{
		public Selector()
		{
		}

		public Selector(string expression)
		{
			Expression = expression;
		}

		public Selector(SelectorType type, string expression)
		{
			Type = type;
			Expression = expression;
		}

		public SelectorType Type { get; set; } = SelectorType.XPath;
		public string Expression { get; set; }
	}
}
