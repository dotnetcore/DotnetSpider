using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DisplaceFormater : Formatter
	{
		public dynamic EqualValue { get; set; }
		public dynamic Displacement { get; set; }

		protected override dynamic FormateValue(dynamic value)
		{
			return value.Equals(EqualValue) ? Displacement : value;
		}

		protected override void CheckArguments()
		{
		}
	}
}
