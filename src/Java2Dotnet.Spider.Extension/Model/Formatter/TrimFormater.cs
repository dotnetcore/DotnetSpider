using System;

namespace Java2Dotnet.Spider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class TrimFormater : Formatter
	{
		public override string Name { get; internal set; } = "TrimFormatter";

		public override string Formate(string value)
		{
			return value?.Trim();
		}
	}
}
