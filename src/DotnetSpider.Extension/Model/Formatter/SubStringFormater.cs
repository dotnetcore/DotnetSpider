using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class SubStringFormatter : Formatter
	{
		public string Start { get; set; }
		public string End { get; set; }
		public int StartOffset { get; set; } = 0;
		public int EndOffset { get; set; } = 0;

		protected override dynamic FormateValue(dynamic value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}

			int begin = value.IndexOf(Start, StringComparison.Ordinal);
			int length;
			if (!string.IsNullOrEmpty(End))
			{
				int end = value.IndexOf(End, begin, StringComparison.Ordinal);
				length = end - begin;
			}
			else
			{
				length = value.Length - begin;
			}

			begin += StartOffset;
			length -= StartOffset;
			length -= EndOffset;
			if (!string.IsNullOrEmpty(End))
			{
				length += End.Length;
			}
			return value.Substring(begin, length);
		}

		protected override void CheckArguments()
		{
		}
	}
}
