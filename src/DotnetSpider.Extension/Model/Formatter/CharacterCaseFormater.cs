using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property)]
	public class CharacterCaseFormatter : Formatter
	{
		public bool ToUpper { get; set; } = true;

		protected override dynamic FormateValue(dynamic value)
		{
			string tmp = value.ToString();

			if (string.IsNullOrEmpty(tmp))
			{
				return ValueWhenNull;
			}

			if (ToUpper)
			{
				return tmp.ToUpperInvariant();
			}
			else
			{
				return tmp.ToLowerInvariant();
			}
		}

		protected override void CheckArguments()
		{
		}
	}
}
