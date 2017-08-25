using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Infrastructure
{
	public class Cache
	{
		private static readonly Dictionary<string, dynamic> Cached = new Dictionary<string, dynamic>();
		private static readonly Lazy<Cache> MyInstance = new Lazy<Cache>(() => new Cache());
		private static readonly object Locker = new object();

		private Cache()
		{
		}

		public static Cache Instance => MyInstance.Value;

		public void Set(string key, dynamic data)
		{
			lock (Locker)
			{
				Cached.Add(key, data);
			}
		}

		public dynamic Get(string key)
		{
			lock (Locker)
			{
				var result = Cached.ContainsKey(key) ? Cached[key] : null;
				return result;
			}
		}
	}
}
