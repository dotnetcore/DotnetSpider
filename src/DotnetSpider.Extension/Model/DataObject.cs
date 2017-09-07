using System;
using System.Collections;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Model
{
	public class DataObject : IDictionary<string, object>
	{
		private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

		public object this[string key]
		{
			get
			{
				var result = _data.ContainsKey(key) ? _data[key] : null;
				return result;
			}
			set => Add(key, value);
		}

		public ICollection<string> Keys => _data.Keys;

		public ICollection<object> Values => _data.Values;

		public int Count => _data.Keys.Count;

		public bool IsReadOnly => false;

		public void Add(string key, object value)
		{
			if (_data.ContainsKey(key))
			{
				_data[key] = value;
			}
			else
			{
				_data.Add(key, value);
			}
		}

		public object GetValue(string key)
		{
			return this[key];
		}

		public void Add(KeyValuePair<string, object> item)
		{
			Add(item.Key, item.Value);
		}

		public void Clear()
		{
			_data.Clear();
		}

		public bool Contains(KeyValuePair<string, object> item)
		{
			var value = this[item.Key];
			return value == item.Value;
		}

		public bool ContainsKey(string key)
		{
			return _data.ContainsKey(key);
		}

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		public bool Remove(string key)
		{
			return _data.Remove(key);
		}

		public bool Remove(KeyValuePair<string, object> item)
		{
			if (Contains(item))
			{
				return Remove(item.Key);
			}
			else
			{
				throw new ArgumentException("item is not exists.");
			}
		}

		public bool TryGetValue(string key, out object value)
		{
			value = this[key];
			return true;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _data.GetEnumerator();
		}
	}
}
