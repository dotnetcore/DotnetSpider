using System;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DigitUnitFormater : Formatter
	{
		public string UnitStringForShi { get; set; } = "十";
		public string UnitStringForBai { get; set; } = "百";
		public string UnitStringForQian { get; set; } = "千";
		public string UnitStringForWan { get; set; } = "万";
		public string UnitStringForYi { get; set; } = "亿";
		public List<string> CustomUnitString { get; set; } = new List<string>();
		public List<long> CustomUnitValue { get; set; } = new List<long>();

		public string NumFormat { get; set; } = "F0";

		protected override dynamic FormateValue(dynamic value)
		{
			try
			{
				string tmp = value.ToString();
				if (string.IsNullOrEmpty(tmp))
				{
					return ValueWhenNull;
				}
				decimal num = decimal.Parse(RegexUtil.DecimalRegex.Match(tmp).ToString());
				if (tmp.EndsWith(UnitStringForShi))
				{
					num = num * 10;
				}
				else if (tmp.EndsWith(UnitStringForBai))
				{
					num = num * 100;
				}
				else if (tmp.EndsWith(UnitStringForBai))
				{
					num = num * 100;
				}
				else if (tmp.EndsWith(UnitStringForQian))
				{
					num = num * 1000;
				}
				else if (tmp.EndsWith(UnitStringForWan))
				{
					num = num * 10000;
				}
				else if (tmp.EndsWith(UnitStringForYi))
				{
					num = num * 100000000;
				}

				if (CustomUnitString.Count > 0)
				{
					for (int i = 0; i < CustomUnitString.Count; i++)
					{
						if (tmp.EndsWith(CustomUnitString[i]))
						{
							num = num * CustomUnitValue[i];
						}
					}
				}
				return num.ToString(NumFormat);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				return ValueWhenNull;
			}
		}

		protected override void CheckArguments()
		{
			if (CustomUnitValue.Count != CustomUnitString.Count)
			{
				throw new Exception("Each unit should have a value.");
			}
		}
	}
}
