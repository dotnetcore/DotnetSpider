using System.Collections.Generic;

namespace DotnetSpider.Core.Selector
{
	public class RegexResult
	{
		private readonly List<string> _groups;
		public static RegexResult EmptyResult = new RegexResult();
		public string Expression { get; set; }

		private RegexResult()
		{
		}

		public RegexResult(string expression, List<string> groups)
		{
			_groups = groups;
			Expression = expression;
		}

		public string Get(int groupId)
		{
			if (_groups != null && _groups.Count > groupId)
			{
				return _groups?[groupId];
			}
			return null;
		}
	}
}
