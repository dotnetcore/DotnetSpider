using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core;
using DotnetSpider.Extension.Utils;
using DotnetSpider.Extension.Model.Formatter;
using System.Linq;
using Newtonsoft.Json.Linq;

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