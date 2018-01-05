using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// 缓存
	/// </summary>
	public class Cache
	{
		private static readonly Dictionary<string, dynamic> Cached = new Dictionary<string, dynamic>();
		private static readonly Lazy<Cache> MyInstance = new Lazy<Cache>(() => new Cache());

		private Cache()
		{
		}

		/// <summary>
		/// 缓存单例对象
		/// </summary>
		public static Cache Instance => MyInstance.Value;

		/// <summary>
		/// 设置缓存
		/// </summary>
		/// <param name="key">索引</param>
		/// <param name="data">数据对象</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Set(string key, dynamic data)
		{
			Cached.Add(key, data);
		}

		/// <summary>
		/// 取缓存数据
		/// </summary>
		/// <param name="key">索引</param>
		/// <returns>数据对象</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public dynamic Get(string key)
		{
			var result = Cached.ContainsKey(key) ? Cached[key] : null;
			return result;
		}
	}
}
