using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class SubStringFormatter : Formatter
	{
		public string StartString { get; set; }
		public string EndString { get; set; }
		public int StartOffset { get; set; } = 0;
		public int EndOffset { get; set; } = 0;

		public override string Formate(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}

			int begin = value.IndexOf(StartString, StringComparison.Ordinal);
			int length;
			if (!string.IsNullOrEmpty(EndString))
			{
				int end = value.IndexOf(EndString, begin, StringComparison.Ordinal);
				length = end - begin;
			}
			else
			{
				length = value.Length - begin;
			}

			begin += StartOffset;
			length -= StartOffset;
			length -= EndOffset;
			if (!string.IsNullOrEmpty(EndString))
			{
				length += EndString.Length;
			}
			return value.Substring(begin, length);
		}
	}
}
