using System;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class FormatStringFormater : Formatter
	{
		public string Format { get; set; }

		protected override dynamic FormateValue(dynamic value)
		{
			return string.Format(Format, value);
		}

		protected override void CheckArguments()
		{
			if (string.IsNullOrEmpty(Format) || string.IsNullOrWhiteSpace(Format))
			{
				throw new SpiderException("Format should not be null or empty.");
			}
		}
	}
}
