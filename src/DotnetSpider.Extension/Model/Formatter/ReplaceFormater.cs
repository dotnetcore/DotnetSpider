using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class ReplaceFormatter : Formatter
	{
		public string OldValue { get; set; }
		public string NewValue { get; set; }

		protected override dynamic FormateValue(dynamic value)
		{
			string tmp = value.ToString();
			return tmp.Replace(OldValue, NewValue);
		}

		protected override void CheckArguments()
		{
		}
	}
}
