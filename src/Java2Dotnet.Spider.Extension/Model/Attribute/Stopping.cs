using System;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Utils;

namespace Java2Dotnet.Spider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Class)]
	public class Stopping : System.Attribute
	{
		public string PropertyName { get; set; }

		public Operate Operate { get; set; }

		public string CompareValue { get; set; }

		internal string DataType { get; set; }

		public bool NeedStop(string value)
		{
			if (RegexUtil.StringTypeRegex.IsMatch(DataType))
			{
				if (Operate == Operate.Equal)
				{
					return value == CompareValue;
				}
				else
				{
					return String.CompareOrdinal(value, CompareValue) > 0;
				}
			}

			if ("time" == DataType || "date" == DataType)
			{
				DateTime realTimeValue;
				DateTime compTimeareValue;
				if (DateTime.TryParse(value, out realTimeValue) && DateTime.TryParse(CompareValue, out compTimeareValue))
				{
					switch (Operate)
					{
						case Operate.Equal:
							{
								return Equals(realTimeValue, compTimeareValue);
							}
						case Operate.Large:
							{
								return realTimeValue > compTimeareValue;
							}
						case Operate.Less:
							{
								return realTimeValue < compTimeareValue;
							}
					}
				}
				else
				{
					throw new SpiderExceptoin($"Can't parse to DateTime: Value-{value}, CompareValue-{CompareValue}");
				}
			}

			if ("text" == DataType)
			{
				if (Operate == Operate.Equal)
				{
					return value == CompareValue;
				}
				else
				{
					return String.CompareOrdinal(value, CompareValue) > 0;
				}
			}


			double realValue = 0;
			double compareValue = 0;
			if (double.TryParse(value, out realValue) && double.TryParse(CompareValue, out compareValue))
			{
				switch (Operate)
				{
					case Operate.Equal:
						{
							return Equals(realValue, compareValue);
						}
					case Operate.Large:
						{
							return realValue > compareValue;
						}
					case Operate.Less:
						{
							return realValue < compareValue;
						}
				}
			}

			throw new SpiderExceptoin($"Can't parse to double: Value-{value}, CompareValue-{CompareValue}");
		}

		public override string ToString()
		{
			return $"{PropertyName} {Operate} {CompareValue}";
		}
	}

	public enum Operate
	{
		Equal,
		Large,
		Less
	}
}
