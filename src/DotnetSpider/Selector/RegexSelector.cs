using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

#if NETSTANDARD20
using System.Reflection;
#endif

namespace DotnetSpider.Selector
{
	/// <summary>
	/// 正则查询器
	/// </summary>
	public class RegexSelector : ISelector
	{
		private readonly string _pattern;
		private readonly Regex _regex;
		private readonly int _group;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="pattern">正则表达式</param>
		/// <param name="group"></param>
		public RegexSelector(string pattern, int group = 0)
		{
			if (string.IsNullOrWhiteSpace(pattern))
			{
				throw new ArgumentException("regex must not be empty");
			}
			_pattern = pattern;
			_regex = new Regex(pattern, RegexOptions.IgnoreCase);
			_group = group;
		}

		/// <summary>
		/// 从文本中查询单个结果
		/// 如果符合条件的结果有多个, 仅返回第一个
		/// </summary>
		/// <param name="text">需要查询的文本</param>
		/// <returns>查询结果</returns>
		public dynamic Select(dynamic text)
		{
			if (text == null)
			{
				return null;
			}

			var type = (Type)text.GetType();
			StringBuilder builder = new StringBuilder();
			if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				foreach (var l in (IEnumerable)text)
				{
					builder.Append(GetElementContent(l));
				}
			}
			else
			{
				builder.Append(GetElementContent(text));
			}

			Match match = _regex.Match(builder.ToString());
			if (match.Success)
			{
				return match.Groups.Count > _group ? match.Groups[_group].Value : null;
			}
			return null;
		}

		/// <summary>
		/// 从文本中查询所有结果
		/// </summary>
		/// <param name="text">需要查询的文本</param>
		/// <returns>查询结果</returns>
		public IEnumerable<dynamic> SelectList(dynamic text)
		{
			if (text == null)
			{
				return null;
			}

			var type = (Type)text.GetType();

			StringBuilder builder = new StringBuilder();

			if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				foreach (var l in (IEnumerable)text)
				{
					builder.Append(GetElementContent(l));
				}
			}
			else
			{
				builder.Append(GetElementContent(text));
			}

			var matches = _regex.Matches(builder.ToString());

			List<dynamic> results = new List<dynamic>();
			foreach (Match match in matches)
			{
				if (match.Groups.Count > _group)
				{
					results.Add(match.Groups[_group].Value);
				}
			}
			return results;
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString() => _pattern;

		private string GetElementContent(dynamic el)
		{
			switch (el)
			{
				case string _:
					return el;
				case HtmlDocument _:
					return ((HtmlDocument)el).DocumentNode.InnerHtml;
				case HtmlNode _:
					return ((HtmlNode)el).InnerHtml;
				default:
					return el.ToString();
			}
		}
	}
}
