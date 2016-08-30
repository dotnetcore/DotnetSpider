using System;
using System.Text.RegularExpressions;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RegexAppendFormatter : Formatter
	{
		public string Pattern { get; set; }

		public string AppendValue { get; set; }

		protected override dynamic FormateValue(dynamic value)
		{
			string tmp = value.ToString();
			if (Regex.IsMatch(tmp, Pattern))
			{
				return $"{tmp}{AppendValue}";
			}
			return value;
		}

		protected override void CheckArguments()
		{
			if (string.IsNullOrEmpty(Pattern) || string.IsNullOrWhiteSpace(Pattern))
			{
				throw new SpiderException("Pattern should not be null or empty.");
			}

			if (string.IsNullOrEmpty(AppendValue) || string.IsNullOrWhiteSpace(AppendValue))
			{
				throw new SpiderException("Append should not be null or empty.");
			}
		}
	}
}
