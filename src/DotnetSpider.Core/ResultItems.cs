using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 存储页面解析的数据结果
	/// 此对象包含在页面对象中, 并传入数据管道被处理
	/// </summary>
	public class ResultItems
	{
		private readonly Dictionary<string, dynamic> _fields = new Dictionary<string, dynamic>();

		/// <summary>
		/// 读取所有数据结果
		/// </summary>
		public IReadOnlyDictionary<string, dynamic> Results => _fields;

		/// <summary>
		/// 当前解析结果对应的目标链接信息
		/// </summary>
		public Request Request { get; set; }

		/// <summary>
		/// 存储的数据结果是否为空
		/// </summary>
		public bool IsEmpty => _fields.Count == 0;

		/// <summary>
		/// 通过键值取得数据结果
		/// </summary>
		/// <param name="key">键值</param>
		/// <returns>数据结果</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public dynamic GetResultItem(string key)
		{
			return _fields.ContainsKey(key) ? _fields[key] : null;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddOrUpdateResultItem(string key, dynamic value)
		{
			if (_fields.ContainsKey(key))
			{
				_fields[key] = value;
			}
			else
			{
				_fields.Add(key, value);
			}
		}
	}
}