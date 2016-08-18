using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class StringFormatFormater : Formatter
	{
		public string Format { get; set; }

		public override string Formate(string value)
		{
			return string.Format(Format, value);
		}
	}
}
