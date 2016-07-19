using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Ioc
{
	public class ServiceProvider
	{
		private static Dictionary<string, List<dynamic>> _cache = new Dictionary<string, List<dynamic>>();

		public static void Add<T>(T t) where T : IService
		{
			if (t == null)
			{
				throw new ArgumentNullException();
			}

			Type type = typeof(T);

			if (_cache.ContainsKey(type.FullName))
			{
				_cache[type.FullName].Add(t);
			}
			else
			{
				var tmp = new List<dynamic>() { t };
				_cache.Add(type.FullName, tmp);
			}
		}

		public static List<T> Get<T>()
		{
			Type type = typeof(T);
			if (_cache.ContainsKey(type.FullName))
			{
				return _cache[type.FullName].Select(t => (T)t).ToList();
			}
			else
			{
				throw new Exception("Can't find service for type: " + type);
			}
		}
	}
}
