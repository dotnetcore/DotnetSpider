using System;
using System.Text.RegularExpressions;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RegexFormatter : Formatter
	{
		private const string ID = "227a207a28024b1cbee3754e76443df2";
		public string Pattern { get; set; }
		public string True { get; set; } = ID;
		public string False { get; set; } = ID;

		protected override dynamic FormateValue(dynamic value)
		{
			string tmp = value.ToString();
			MatchCollection matches = Regex.Matches(tmp, Pattern);
			return matches.Count > 0 ? (True == ID ? matches[0].Value : True) : (False == ID ? tmp : False);
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
