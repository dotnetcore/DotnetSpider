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

		protected override object FormateValue(object value)
		{
			switch (Type)
			{
				case TrimType.All:
					{
						return value.ToString().Trim();
					}
				case TrimType.Left:
					{
						return value.ToString().TrimStart();
					}
				case TrimType.Right:
					{
						return value.ToString().TrimEnd();
					}
				default:
					{
						return value.ToString().Trim();
					}
			}
		}

		protected override void CheckArguments()
		{
		}
	}
}
