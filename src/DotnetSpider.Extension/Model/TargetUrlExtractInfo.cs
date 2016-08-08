using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core.Selector;

namespace DotnetSpider.Extension.Model
{
	public class TargetUrlExtractor
	{
		public List<Regex> Patterns { get; set; } = new List<Regex>();
		public List<Formatter.Formatter> Formatters { get; set; }
		public ISelector Region { get; set; }
	}
}
