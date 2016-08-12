using System;
using System.Text.RegularExpressions;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RegexMatchFormatter : Formatter
	{
		public string Pattern { get; set; }

		public override string Formate(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}
			return Regex.Match(value,Pattern).ToString();
		}
	}
}
