using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property)]
	public class CharacterCaseFormatter : Formatter
	{
		public bool ToUpper { get; set; } = true;

		protected override object FormateValue(object value)
		{
			return ToUpper ? value.ToString().ToUpperInvariant() : value.ToString().ToLowerInvariant();
		}

		protected override void CheckArguments()
		{
		}
	}
}
