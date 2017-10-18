using System;
using System.Collections;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class StringFormater : Formatter
	{
		public string Format { get; set; }

		protected override object FormateValue(object value)
		{
			return string.Format(Format, value.ToString());
		}

		protected override void CheckArguments()
		{
			if (string.IsNullOrEmpty(Format) || string.IsNullOrWhiteSpace(Format))
			{
				throw new SpiderException("FormatString should not be null or empty.");
			}
		}
	}
}
