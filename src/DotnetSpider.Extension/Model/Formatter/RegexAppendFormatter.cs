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

		protected override object FormateValue(object value)
		{
			var tmp = value.ToString();
			return Regex.IsMatch(tmp, Pattern) ? $"{tmp}{AppendValue}" : tmp;
		}

		protected override void CheckArguments()
		{
			if (string.IsNullOrWhiteSpace(Pattern))
			{
				throw new SpiderException("Pattern should not be null or empty.");
			}

			if (string.IsNullOrWhiteSpace(AppendValue))
			{
				throw new SpiderException("Append should not be null or empty.");
			}
		}
	}
}
