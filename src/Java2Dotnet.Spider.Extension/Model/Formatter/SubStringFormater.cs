using System;

namespace Java2Dotnet.Spider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class SubStringFormatter : Formatter
	{
		public override string Name { get; internal set; } = "SubStringFormatter";

		public string StartString { get; set; }
		public string EndString { get; set; }
		public int StartOffset { get; set; } = 0;
		public int EndOffset { get; set; } = 0;

		public override string Formate(string value)
		{
			int begin = value.IndexOf(StartString, StringComparison.Ordinal);
			int end = value.IndexOf(EndString, begin, StringComparison.Ordinal);
			int length = end - begin;

			begin += StartOffset;
			length -= StartOffset;
			length -= EndOffset;
			length += EndString.Length;

			return value.Substring(begin, length);
		}
	}
}
