using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotnetSpider.Common
{
	public class Headers : IEnumerable<KeyValuePair<string, string>>
	{
		private readonly Dictionary<string, string> _keyValues = new Dictionary<string, string>();

		public string this[string key] => _keyValues.ContainsKey(key) ? _keyValues[key] : null;

		public int Count => _keyValues.Count;

		public void Clear()
		{
			_keyValues.Clear();
		}

		public bool ContainsKey(string key)
		{
			return _keyValues.ContainsKey(key);
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return _keyValues.GetEnumerator();
		}

		public bool Remove(string key)
		{
			return _keyValues.Remove(key);
		}

		public bool TryGetValue(string key, out string value)
		{
			return _keyValues.TryGetValue(key, out value);
		}

		public void Add(string key, string value)
		{
			if (_keyValues.ContainsKey(key))
			{
				_keyValues[key] = value;
			}
			else
			{
				_keyValues.Add(key, value);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _keyValues.GetEnumerator();
		}
	}
}
