using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Extension.Common;
using DotnetSpider.Extension.Model.Formatter;

namespace DotnetSpider.Extension.Configuration
{
	public class TargetUrlExtractor
	{
		public List<string> Patterns { get; set; } = new List<string>();
		public List<Formatter> Formatters { get; set; }
		public Selector Region { get; set; }

		internal Model.TargetUrlExtractor GetTargetUrlExtractInfo()
		{
			var t = new Model.TargetUrlExtractor
			{
				Formatters = Formatters,
				Region = SelectorUtil.Parse(Region)
			};
			foreach (var p in Patterns)
			{
				if (!string.IsNullOrEmpty(p?.Trim()))
				{
					t.Patterns.Add(new Regex(p));
				}
			}
			return t;
		}
	}
}