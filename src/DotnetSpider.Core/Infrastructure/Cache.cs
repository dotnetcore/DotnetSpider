using System;
using System.Collections.Concurrent;

namespace DotnetSpider.Core.Infrastructure
{
	public class Cache
	{
		private ConcurrentDictionary<string, dynamic> _cache = new ConcurrentDictionary<string, dynamic>();

		private Cache()
		{

		}

		private static readonly Lazy<Cache> _instance = new Lazy<Cache>(() =>
		{
			return new Cache();
		});

		public static Cache Instance
		{
			get { return _instance.Value; }
		}

		public void Set(string key, dynamic data)
		{
			_cache.TryAdd(key, data);
		}

		public dynamic Get(string key)
		{
			dynamic result;
			if (_cache.TryGetValue(key, out result))
			{
				return result;
			}
			else
			{
				return null;
			}
		}
	}
}
