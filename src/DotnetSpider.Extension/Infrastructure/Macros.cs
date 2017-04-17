using System;

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// 代码用户直接调用此类的值用于拼接URL
	/// http://hotel.meituan.com/api/getcounterandpois/{city.NameEn}?ci={MACROS_TODAY},co={MACROS_TOMORROW},sort=,w=,page=1,attrs=20022:20036
	/// 普通用户提供界面选择Macros
	/// </summary>
	public static class Macros
	{
		public const string MacrosToday = "{MACROS_TODAY}";
		public const string MacrosTomorrow = "{MACROS_TOMORROW}";
		public const string MacrosFirstDayofThisMonth = "{MACROS_FIRST_DAY_OF_THIS_MONTH}";

		public static string Replace(string str)
		{
			return str.Replace(MacrosToday, DateTime.Now.ToString("yyyy-MM-dd"));
		}
	}
}
