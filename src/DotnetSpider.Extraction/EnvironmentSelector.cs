using System;
using System.Collections.Generic;

namespace DotnetSpider.Extraction
{
	public class EnviromentFields
	{
		public const string Index = "INDEX_00260C60";
	}

	/// <summary>
	/// 环境变量值查询, 在Request对象中, 可以存入一些初始字典供查询
	/// 还可以查询如: 当天时间等
	/// 此类不需要具体实现, 仅作为标识使用
	/// </summary>
	public class EnvironmentSelector : ISelector
	{
		/// <summary>
		/// 查询的键值
		/// </summary>
		public string Field { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="field">查询的键值</param>
		public EnvironmentSelector(string field)
		{
			Field = field;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public dynamic Select(dynamic text)
		{
			throw new NotSupportedException("EnvironmentSelector is only used as a switch.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public IEnumerable<dynamic> SelectList(dynamic text)
		{
			throw new NotSupportedException("EnvironmentSelector is only used as a switch.");
		}
	}
}
