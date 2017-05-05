using DotnetSpider.Core.Selector;

namespace DotnetSpider.Extension.Model
{
	public class Selector : System.Attribute
	{
		public SelectorType Type { get; set; }
		public string Expression { get; set; }
	}
}
