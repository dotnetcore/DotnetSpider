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
		private readonly Dictionary<string, dynamic> _fields = new Dictionary<string, dynamic>();

		/// <summary>
		/// 读取所有数据结果
		/// </summary>
		public IReadOnlyDictionary<string, dynamic> Results => _fields;

		/// <summary>
		/// 对应的目标链接信息
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

		/// <summary>
		/// 添加或更新数据结果
		/// </summary>
		/// <param name="key">键值</param>
		/// <param name="value">数据结果</param>
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