using System;
using System.Collections.Generic;

namespace Java2Dotnet.Spider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class ClassifyFormatter : Formatter
	{
		public enum JudgementOption
		{
			Equal,
			Contain
		}
		public override string Name { get; internal set; } = "ClassifyFormatter";

		public Dictionary<List<string>, string> MatchPatterns { get; set; } = new Dictionary<List<string>, string>();

		public JudgementOption Judgement { get; set; } = JudgementOption.Equal;
		public string FalseString { get; set; } = "";
		public string NullString { get; set; } = string.Empty;

		public override string Formate(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return NullString;
			}

			foreach (var pair in MatchPatterns)
			{
				foreach (var s in pair.Key)
				{
					if (Judgement == JudgementOption.Equal)
					{
						if (s == value)
						{
							return pair.Value;
						}
					}
					else if (Judgement == JudgementOption.Contain)
					{
						if (value.Contains(s))
						{
							return pair.Value;
						}
					}
				}
			}
			return string.Empty;
		}
	}
}
