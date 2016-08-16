using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Common;

namespace DotnetSpider.Extension.Model
{
	public class TargetUrlExtractor
	{
		private Selector _regionSelector;
		private List<string> _patterns;

		public List<string> Patterns
		{
			get { return _patterns; }
			set
			{
				if (value != null)
				{
					if (!Equals(value, _patterns))
					{
						_patterns = value;
						Regexes = value.Select(s => new Regex(s)).ToList();
					}
				}
				else
				{
					_patterns = null;
					Regexes = new List<Regex>();
				}
			}
		}

		public List<Formatter.Formatter> Formatters { get; set; }

		public Selector Region
		{
			get { return _regionSelector; }
			set
			{
				if (value != null)
				{
					if (!Equals(value, _regionSelector))
					{
						_regionSelector = value;
						RegionSelector = SelectorUtil.Parse(_regionSelector);
					}
				}
				else
				{
					_regionSelector = null;
					RegionSelector = null;
				}
			}
		}

		internal ISelector RegionSelector { get; private set; }
		internal List<Regex> Regexes { get; private set; }
	}
}
