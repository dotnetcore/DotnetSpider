using System.Collections.Generic;

namespace Java2Dotnet.Spider.Core.Selector
{
	public class RegexResult
	{
		private readonly List<string> _groups;
		public static RegexResult EmptyResult = new RegexResult();
		private readonly string _regexString;

		private RegexResult()
		{
		}

		public RegexResult(string regexString, List<string> groups)
		{
			_groups = groups;
			_regexString = regexString;
		}

		public string Get(int groupId)
		{
			if (_groups.Count > groupId)
			{
				return _groups?[groupId];
			}
			throw new SpiderExceptoin($"Regex: {_regexString} can't match data, please verify your regex or the page's source html.");
		}
	}
}
