using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotnetSpider.Core.Infrastructure;
#if NET_CORE
using System.Reflection;
#endif

namespace DotnetSpider.Core.Selector
{
	public class RegexSelector : ISelector
	{
		private readonly string _pattern;
		private readonly Regex _regex;
		private readonly int _group;

		public RegexSelector(string pattern, int group)
		{
			if (string.IsNullOrEmpty(pattern))
			{
				throw new ArgumentException("regex must not be empty");
			}
			// Check bracket for regex group. Add default group 1 if there is no group.
			// Only check if there exists the valid left parenthesis, leave regexp validation for Pattern.
			if (StringExtensions.CountMatches(pattern, "(") - StringExtensions.CountMatches(pattern, "\\(") ==
					StringExtensions.CountMatches(pattern, "(?:") - StringExtensions.CountMatches(pattern, "\\(?:"))
			{
				pattern = "(" + pattern + ")";
			}
			_pattern = pattern;
			//check: regex = Pattern.compile(regexStr, Pattern.DOTALL | Pattern.CASE_INSENSITIVE);
			_regex = new Regex(pattern, RegexOptions.IgnoreCase);
			_group = group;
		}

		public RegexSelector(string regexStr)
			: this(regexStr, 0)
		{
		}

		public dynamic Select(dynamic text)
		{
			if (text == null)
			{
				return null;
			}

			var type = (Type)text.GetType();
			string tmp = "";
#if NET_CORE
			if (typeof(ICollection).GetTypeInfo().IsAssignableFrom(type))
#else
			if (typeof(ICollection).IsAssignableFrom(type))
#endif			
			{
				foreach (var l in (IEnumerable)text)
				{
					tmp += l.ToString();
				}
			}
			else
			{
				tmp = text is string ? text : text.ToSting();
			}
			return SelectGroup(tmp).Get(_group);
		}

		public IEnumerable<dynamic> SelectList(dynamic text)
		{
			if (text == null)
			{
				return null;
			}

			var type = (Type)text.GetType();
			string tmp = "";

			if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				foreach (var l in (IEnumerable)text)
				{
					tmp += l.ToString();
				}
			}
			else
			{
				tmp = text.ToSting();
			}

			IList<RegexResult> results = SelectGroupList(tmp);
			return results.Select(result => result.Get(_group)).Cast<dynamic>().ToList();
		}

		public override string ToString()
		{
			return _pattern;
		}

		private RegexResult SelectGroup(string text)
		{
			var match = _regex.Match(text);
			if (match.Success)
			{
				return new RegexResult(_regex.ToString(), (from Group g in match.Groups select g.Value).ToList());
			}
			else
			{
				return new RegexResult(null, null);
			}
		}

		private List<RegexResult> SelectGroupList(string text)
		{
			List<RegexResult> resultList = new List<RegexResult>();

			var matches = _regex.Matches(text);
			if (matches.Count > 0)
			{
				foreach (Match m in matches)
				{
					resultList.Add(new RegexResult(_regex.ToString(), (from Group @group in m.Groups select @group.Value).ToList()));
				}
			}

			return resultList;
		}
	}
}
