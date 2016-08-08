using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class ReplaceFormatter : Formatter
	{
		public override string Name { get; internal set; } = "ReplaceFormatter";

		public string OldValue { get; set; }
		public string NewValue { get; set; }

		public override string Formate(string value)
		{
			return value?.Replace(OldValue, NewValue);
		}
	}
}
