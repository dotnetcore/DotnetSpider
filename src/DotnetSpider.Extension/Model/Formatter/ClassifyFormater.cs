using System;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class ClassifyFormatter : Formatter
	{
		public enum JudgementOption
		{
			Equal,
			Contain
		}

		public string[] Patterns { get; set; }
		public string[] Values { get; set; }
		public Dictionary<string, string> MatchPatterns { get; internal set; } = new Dictionary<string, string>();

		public JudgementOption Judgement { get; set; } = JudgementOption.Equal;
		public string FalseString { get; set; } = "";
		public string NullString { get; set; } = string.Empty;

		public override string Formate(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return NullString;
			}

			if (Patterns.Length != Values.Length)
			{
				throw new Exception("Patterns' Count Must Equal to Values' Count!");
			}
			if (MatchPatterns.Count == 0)
			{
				for (int i = 0; i < Patterns.Length; ++i)
				{
					MatchPatterns.Add(Patterns[i], Values[i]);
				}
			}

			foreach (var pair in MatchPatterns)
			{

				if (Judgement == JudgementOption.Equal)
				{
					if (pair.Key == value)
					{
						return pair.Value;
					}
				}
				else if (Judgement == JudgementOption.Contain)
				{
					if (value.Contains(pair.Key))
					{
						return pair.Value;
					}
				}

			}
			return string.Empty;
		}
	}
}
