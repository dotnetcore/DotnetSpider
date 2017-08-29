using System;
using System.Collections;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Model
{
	public partial class DataObject : IDictionary<string, object>
	{
		private Dictionary<string, object> data = new Dictionary<string, object>();

		public DataObject()
		{
		}

		public object this[string key]
		{
			get
			{
				var result = data.ContainsKey(key) ? data[key] : null;
				return result;
			}
			set
			{
				Add(key, value);
			}
		}

		public ICollection<string> Keys => data.Keys;

		public ICollection<object> Values => data.Values;

		public int Count => data.Keys.Count;

		public bool IsReadOnly => false;

		public void Add(string key, object value)
		{
			if (data.ContainsKey(key))
			{
				data[key] = value;
			}
			else
			{
				data.Add(key, value);
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
			data.Clear();
		}

		public bool Contains(KeyValuePair<string, object> item)
		{
			var value = this[item.Key];
			return value == item.Value;
		}

		public bool ContainsKey(string key)
		{
			return data.ContainsKey(key);
		}

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return data.GetEnumerator();
		}

		public bool Remove(string key)
		{
			return data.Remove(key);
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
			return data.GetEnumerator();
		}
	}
}
