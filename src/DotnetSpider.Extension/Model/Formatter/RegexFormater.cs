using System;
using System.Text.RegularExpressions;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RegexFormatter : Formatter
	{
		public string Pattern { get; set; }

		protected override dynamic FormateValue(dynamic value)
		{
			return Regex.Match(value,Pattern).ToString();
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
