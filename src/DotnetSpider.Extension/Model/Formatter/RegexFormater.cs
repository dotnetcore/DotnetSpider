using System;
using System.Text.RegularExpressions;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RegexFormatter : Formatter
	{
		private const string Id = "227a207a28024b1cbee3754e76443df2";
		public string Pattern { get; set; }
		public string True { get; set; } = Id;
		public string False { get; set; } = Id;

		protected override dynamic FormateValue(dynamic value)
		{
			string tmp = value.ToString();
			if (string.IsNullOrEmpty(tmp))
			{
				return ValueWhenNull;
			}
			MatchCollection matches = Regex.Matches(tmp, Pattern);
			return matches.Count > 0 ? (True == Id ? matches[0].Value : True) : (False == Id ? tmp : False);
		}

		protected override void CheckArguments()
		{
			if (string.IsNullOrEmpty(Pattern) || string.IsNullOrWhiteSpace(Pattern))
			{
				throw new SpiderException("Pattern should not be null or empty.");
			}
		}
	}
}
