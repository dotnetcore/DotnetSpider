using System;
using System.Collections.Concurrent;

namespace DotnetSpider.Core.Infrastructure
{
	public class Cache
	{
		private readonly ConcurrentDictionary<string, dynamic> _cache = new ConcurrentDictionary<string, dynamic>();

		private Cache()
		{

		}

		private static readonly Lazy<Cache> MyInstance = new Lazy<Cache>(() => new Cache());

		public static Cache Instance => MyInstance.Value;

		public void Set(string key, dynamic data)
		{
			_cache.TryAdd(key, data);
		}

		public dynamic Get(string key)
		{
			dynamic result;
			return _cache.TryGetValue(key, out result) ? result : null;
		}
	}
}
