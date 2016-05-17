using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Utils;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Configuration
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
				Region = SelectorUtil.GetSelector(Region)
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