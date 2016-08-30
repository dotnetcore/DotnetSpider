using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DisplaceFormater : Formatter
	{
		public string EqualValue { get; set; }
		public string Displacement { get; set; }

		protected override dynamic FormateValue(dynamic value)
		{
			string tmp = value.ToString();
			return tmp == EqualValue ? Displacement : tmp;
		}

		protected override void CheckArguments()
		{
		}
	}
}
