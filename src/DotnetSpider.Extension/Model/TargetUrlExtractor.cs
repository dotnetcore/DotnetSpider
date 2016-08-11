using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core.Selector;

namespace DotnetSpider.Extension.Model
{
	public class TargetUrlExtractor
	{
		public List<string> Patterns { get; set; } = new List<string>();
		public List<Formatter.Formatter> Formatters { get; set; }
		public Selector Region { get; set; }
	}
}
