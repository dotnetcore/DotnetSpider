using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DisplaceFormater : Formatter
	{
		public string EqualValue { get; set; }

		public string Displacement { get; set; }

		protected override object FormateValue(object value)
		{
			return value.Equals(EqualValue) ? Displacement : value;
		}

		protected override void CheckArguments()
		{
		}
	}
}
