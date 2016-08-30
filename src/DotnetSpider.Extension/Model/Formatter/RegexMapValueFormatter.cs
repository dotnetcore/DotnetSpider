using System;
using System.Text.RegularExpressions;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RegexMapValueFormatter : Formatter
	{
		public string[] Patterns { get; set; }
		public string[] Values { get; set; }

		protected override dynamic FormateValue(dynamic value)
		{
			int length = Patterns.Length;

			for (int i = 0; i < length; ++i)
			{
				string pattern = Patterns[i];
				string patternValue = Values[i];

				if (Regex.Matches(value, pattern).Count > 0)
				{
					return patternValue;
				}
			}

			return string.Empty;
		}

		protected override void CheckArguments()
		{
			if (Patterns == null || Patterns.Length == 0 || Values == null || Values.Length == 0 || Patterns.Length != Values.Length)
			{
				throw new SpiderException("Arguments incorrect. Patterns and Values should not be null, and count should be same.");
			}
		}
	}
}
