using System;
using System.Text.RegularExpressions;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RegexReplaceFormatter : Formatter
	{
		public string Pattern { get; set; }
		public string NewValue { get; set; }

		protected override dynamic FormateValue(dynamic value)
		{
			string tmp = value.ToString();
			if (string.IsNullOrEmpty(tmp))
			{
				return ValueWhenNull;
			}

			if (RetutnDateString)
			{
				return Regex.Replace(tmp, Pattern, DateTime.ToString(DateFormat));
			}
			return Regex.Replace(tmp, Pattern, NewValue);
		}

		protected override void CheckArguments()
		{
			if (NewValue == null)
			{
				NewValue = string.Empty;
			}
			if (string.IsNullOrEmpty(Pattern) || string.IsNullOrWhiteSpace(Pattern))
			{
				throw new SpiderException("Pattern should not be null or empty.");
			}
		}
	}
}
