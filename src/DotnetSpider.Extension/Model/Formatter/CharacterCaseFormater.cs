using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property)]
	public class CharacterCaseFormatter : Formatter
	{
		public bool ToUpper { get; set; } = true;

		protected override dynamic FormateValue(dynamic value)
		{
			if (ToUpper)
			{
				return value.ToUpperInvariant();
			}
			else
			{
				return value.ToLowerInvariant();
			}
		}

		protected override void CheckArguments()
		{
		}
	}
}
