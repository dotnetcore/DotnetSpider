using DotnetSpider.Downloader;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 存储页面解析的数据结果
	/// 此对象包含在页面对象中, 并传入数据管道被处理
	/// </summary>
	public class ResultItems : IDictionary<string, object>
	{
		/// <summary>
		/// 所有数据结果
		/// </summary>
		private readonly Dictionary<string, object> _results = new Dictionary<string, object>();

		public object this[string key]
		{
			get
			{
				lock (this)
				{
					return _results.ContainsKey(key) ? _results[key] : null;
				}
			}
			set
			{
				lock (this)
				{
					if (_results.ContainsKey(key))
					{
						_results[key] = value;
					}

					else
					{
						_results.Add(key, value);
					}
				}
			}
		}

		/// <summary>
		/// 对应的目标链接信息
		/// </summary>
		public Request Request { get; set; }

		/// <summary>
		/// 存储的数据结果是否为空
		/// </summary>
		public bool IsEmpty => _results == null || _results.Count == 0;

		public ICollection<string> Keys => _results.Keys;

		public ICollection<object> Values => _results.Values;

		public int Count => _results.Count;

		public bool IsReadOnly => false;

		public void Add(string key, object value)
		{
			this[key] = value;
		}

		public void Add(KeyValuePair<string, object> item)
		{
			this[item.Key] = item.Value;
		}

		public void Clear()
		{
			_results.Clear();
		}

		public bool Contains(KeyValuePair<string, object> item)
		{
			return _results.ContainsKey(item.Key) && _results[item.Key] == item.Value;
		}

		public bool ContainsKey(string key)
		{
			return _results.ContainsKey(key);
		}

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _results.GetEnumerator();
		}

		public bool Remove(string key)
		{
			return _results.Remove(key);
		}

		public bool Remove(KeyValuePair<string, object> item)
		{
			return _results.Remove(item.Key);
		}

		public bool TryGetValue(string key, out object value)
		{
			value = this[key];
			return value != null;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _results.GetEnumerator();
		}
	}
}