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

			if (RegexUtil.IntTypeRegex.IsMatch(DataType))
			{
				int realValue = 0;
				int compareValue = 0;
				if (int.TryParse(value, out realValue) && int.TryParse(CompareValue, out compareValue))
				{
					switch (Operate)
					{
						case Operate.Equal:
							{
								return realValue == compareValue;
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
				else
				{
					throw new SpiderExceptoin($"Can't parse to int: Value-{value}, CompareValue-{CompareValue}");
				}
			}

			if (RegexUtil.BigIntTypeRegex.IsMatch(DataType))
			{
				long realValue = 0;
				long compareValue = 0;
				if (long.TryParse(value, out realValue) && long.TryParse(CompareValue, out compareValue))
				{
					switch (Operate)
					{
						case Operate.Equal:
							{
								return realValue == compareValue;
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
				else
				{
					throw new SpiderExceptoin($"Can't parse to long: Value-{value}, CompareValue-{CompareValue}");
				}
			}

			if (RegexUtil.FloatTypeRegex.IsMatch(DataType))
			{
				float realValue = 0;
				float compareValue = 0;
				if (float.TryParse(value, out realValue) && float.TryParse(CompareValue, out compareValue))
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
				else
				{
					throw new SpiderExceptoin($"Can't parse to float: Value-{value}, CompareValue-{CompareValue}");
				}
			}

			if (RegexUtil.DoubleTypeRegex.IsMatch(DataType))
			{
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
				else
				{
					throw new SpiderExceptoin($"Can't parse to double: Value-{value}, CompareValue-{CompareValue}");
				}
			}
			if (RegexUtil.DateTypeRegex.IsMatch(DataType) || RegexUtil.TimeStampTypeRegex.IsMatch(DataType))
			{
				DateTime realValue;
				DateTime compareValue;
				if (DateTime.TryParse(value, out realValue) && DateTime.TryParse(CompareValue, out compareValue))
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
				else
				{
					throw new SpiderExceptoin($"Can't parse to DateTime: Value-{value}, CompareValue-{CompareValue}");
				}
			}

			if (RegexUtil.TimeStampTypeRegex.IsMatch(DataType) || RegexUtil.DateTypeRegex.IsMatch(DataType))
			{
				DateTime realValue;
				DateTime compareValue;
				if (DateTime.TryParse(value, out realValue) && DateTime.TryParse(CompareValue, out compareValue))
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

			throw new SpiderExceptoin("Unsport DataType: " + DataType);
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
