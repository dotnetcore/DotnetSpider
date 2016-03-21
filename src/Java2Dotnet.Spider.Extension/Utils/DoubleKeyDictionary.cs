using System.Collections.Generic;

namespace Java2Dotnet.Spider.Extension.Utils
{
	public class DoubleKeyMap<TK1, TK2, TV>
	{
		private Dictionary<TK1, Dictionary<TK2, TV>> _map;

		public DoubleKeyMap()
		{
			Init();
		}

		public DoubleKeyMap(IDictionary<TK1, IDictionary<TK2, TV>> map)
		{
			Init();
		}

		private void Init()
		{
			if (_map == null)
			{
				_map = new Dictionary<TK1, Dictionary<TK2, TV>>();
			}
		}

		public Dictionary<TK2, TV> Get(TK1 key)
		{
			return _map[key];
		}

		public TV Get(TK1 key1, TK2 key2)
		{
			var tmp = Get(key1);
			if (tmp == null)
			{
				return default(TV);
			}
			return tmp.ContainsKey(key2) ? tmp[key2] : default(TV);
		}

		public void Put(TK1 key1, Dictionary<TK2, TV> submap)
		{
			if (_map.ContainsKey(key1))
			{
				_map[key1] = submap;
			}
			else
			{
				_map.Add(key1, submap);
			}
		}

		public void Put(TK1 key1, TK2 key2, TV value)
		{
			if (!_map.ContainsKey(key1))
			{
				_map.Add(key1, new Dictionary<TK2, TV>());
			}
			var tmp = Get(key1);
			if (tmp.ContainsKey(key2))
			{
				tmp[key2] = value;
			}
			else
			{
				tmp.Add(key2, value);
			}
		}

		public void Remove(TK1 key1, TK2 key2)
		{
			if (Get(key1) == null)
			{
				return;
			}

			var tmp = Get(key1);

			if (tmp.ContainsKey(key2))
			{
				tmp.Remove(key2);
			}
		}

		public void Remove(TK1 key1)
		{
			if (_map.ContainsKey(key1))
			{
				_map.Remove(key1);
			}
		}
	}
}
