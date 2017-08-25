using System.Collections.Generic;

namespace DotnetSpider.Core.Selector
{
	public class RegexResult
	{
		public static RegexResult EmptyResult = new RegexResult();

		private readonly List<string> _groups;
		public readonly string Expression;

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
