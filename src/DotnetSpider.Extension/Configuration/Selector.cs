using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Configuration
{
	public class Selector
	{
		public ExtractType Type { get; set; }
		public string Expression { get; set; }
		public object Argument { get; set; }
	}
}
