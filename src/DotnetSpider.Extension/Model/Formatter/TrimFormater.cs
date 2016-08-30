using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class TrimFormater : Formatter
	{
		public enum TrimType
		{
			Right,
			Left,
			All
		}

		public TrimType Type { get; set; } = TrimType.All;

		protected override dynamic FormateValue(dynamic value)
		{
			string tmp = value.ToString();
			switch (Type)
			{
				case TrimType.All:
					{
						return tmp.Trim();
					}
				case TrimType.Left:
					{
						return tmp.TrimStart();
					}
				case TrimType.Right:
					{
						return tmp.TrimEnd();
					}
				default:
					{
						return tmp.Trim();
					}
			}
		}

		protected override void CheckArguments()
		{
		}
	}
}
