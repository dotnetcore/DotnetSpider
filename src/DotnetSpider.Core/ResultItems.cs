using DotnetSpider.Common;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 存储页面解析的数据结果
	/// 此对象包含在页面对象中, 并传入数据管道被处理
	/// </summary>
	public class ResultItems
	{
		/// <summary>
		/// 所有数据结果
		/// </summary>
		public readonly Dictionary<string, dynamic> Results = new Dictionary<string, dynamic>();

		/// <summary>
		/// 对应的目标链接信息
		/// </summary>
		public Request Request { get; set; }

		/// <summary>
		/// 存储的数据结果是否为空
		/// </summary>
		public bool IsEmpty => Results == null || Results.Count == 0;

		/// <summary>
		/// 通过键值取得数据结果
		/// </summary>
		/// <param name="key">键值</param>
		/// <returns>数据结果</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public dynamic GetResultItem(string key)
		{
			return Results.ContainsKey(key) ? Results[key] : null;
		}

		/// <summary>
		/// 添加或更新数据结果
		/// </summary>
		/// <param name="key">键值</param>
		/// <param name="value">数据结果</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddOrUpdateResultItem(string key, dynamic value)
		{
			if (Results.ContainsKey(key))
			{
				Results[key] = value;
			}
			else
			{
				Results.Add(key, value);
			}
		}
	}
}