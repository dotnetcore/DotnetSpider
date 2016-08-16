using System;
using System.Text.RegularExpressions;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RegexAppendFormatter : Formatter
	{
		public string Pattern { get; set; }

		public string Append { get; set; }

		public override string Formate(string value)
		{
			if (Regex.IsMatch(value, Pattern))
			{
				return $"{value}{Append}";
			}
			return value;
		}
	}
}
