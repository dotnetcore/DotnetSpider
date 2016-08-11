using System.Collections.Generic;

namespace DotnetSpider.Extension.Model
{
	public class TargetUrlExtractor
	{
		public List<string> Patterns { get; set; } = new List<string>();
		public List<Formatter.Formatter> Formatters { get; set; }
		public Selector Region { get; set; }
	}
}
