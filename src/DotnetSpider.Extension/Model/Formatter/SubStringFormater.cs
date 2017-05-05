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
			string tmp = value.ToString();

			int begin = tmp.IndexOf(Start, StringComparison.Ordinal);
			int length;
			if (!string.IsNullOrEmpty(End))
			{
				int end = tmp.IndexOf(End, begin, StringComparison.Ordinal);
				length = end - begin;
			}
			else
			{
				length = tmp.Length - begin;
			}

			begin += StartOffset;
			length -= StartOffset;
			length -= EndOffset;
			if (!string.IsNullOrEmpty(End))
			{
				length += End.Length;
			}
			return tmp.Substring(begin, length);
		}

		protected override void CheckArguments()
		{
		}
	}
}
