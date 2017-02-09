using System;
using System.Collections;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class FormatStringFormater : Formatter
	{
		public string Format { get; set; }

		protected override dynamic FormateValue(dynamic value)
		{
			var tmp = value as ICollection;

			if (tmp == null)
			{
				return string.Format(Format, value.ToString());
			}
			else
			{
				ArrayList array =new ArrayList(tmp);
				return string.Format(Format, array.ToArray());
			}
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
