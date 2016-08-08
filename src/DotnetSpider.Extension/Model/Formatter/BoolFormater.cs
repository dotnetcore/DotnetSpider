using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class BoolFormatter : Formatter
	{
		public enum JudgementOption
		{
			Equal,
			Contain
		}
		public override string Name { get; internal set; } = "BoolFormatter";

		public string Pattern { get; set; }
		public JudgementOption Judgement { get; set; } = JudgementOption.Equal;
		public string TrueString { get; set; } = "T";
		public string FalseString { get; set; } = "F";
		public string NullString { get; set; } = string.Empty;

		public override string Formate(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return NullString;
			}

			if (Judgement == JudgementOption.Equal)
			{
				if (Pattern == "")
				{
					if (string.IsNullOrEmpty(value))
					{
						return TrueString;
					}
					return FalseString;
				}
				if (value == Pattern)
				{
					return TrueString;
				}
				return FalseString;
			}

			if (Judgement == JudgementOption.Contain)
			{
				if (value.Contains(Pattern))
				{
					return TrueString;
				}
				return FalseString;
			}
			return string.Empty;
		}
	}
}
