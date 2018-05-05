using DotnetSpider.Core.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Httpclient pool impletion for <see cref="HttpClientPool"/>
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// HttpClient池
	/// </summary>
	public class HttpClientPool : IHttpClientPool
	{
		private AutomicLong _getHttpClientCount = new AutomicLong(0);
		private readonly Dictionary<string, HttpClientEntry> _pool = new Dictionary<string, HttpClientEntry>();

		/// <summary>
		/// Get a <see cref="HttpClientElement"/> from <see cref="IHttpClientPool"/>.
		/// Return same <see cref="HttpClientElement"/> instance when <paramref name="hashCode"/> is same.
		/// This can ensure some pages have same CookieContainer.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 通过不同的Hash分组, 返回对应的HttpClient
		/// 设计初衷: 某些网站会对COOKIE某部分做承上启下的检测, 因此必须保证: www.a.com/keyword=xxxx&amp;page=1 www.a.com/keyword=xxxx&amp;page=2 在同一个HttpClient里访问
		/// </summary>
		/// <param name="hash">分组的哈希 Hashcode to identify different group.</param>
		/// <returns>HttpClientItem</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public HttpClientEntry GetHttpClient(string hash)
		{
			if (string.IsNullOrWhiteSpace(hash))
			{
				hash = string.Empty;
			}
			_getHttpClientCount.Inc();

			if (_getHttpClientCount.Value % 100 == 0)
			{
				CleanupPool();
			}

			if (_pool.ContainsKey(hash))
			{
				_pool[hash].ActiveTime = DateTime.Now;
				return _pool[hash];
			}
			else
			{
				var item = new HttpClientEntry();
				_pool.Add(hash, item);
				return item;
			}
		}

		/// <summary>
		/// Add cookie to <see cref="IHttpClientPool"/>
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 设置 Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddCookie(Cookie cookie)
		{
			foreach (var item in _pool.Values)
			{
				item.Handler.CookieContainer.Add(cookie);
			}
		}

		private void CleanupPool()
		{
			List<string> needRemoveEntries = new List<string>();
			var now = DateTime.Now;
			foreach (var pair in _pool)
			{
				if ((now - pair.Value.ActiveTime).TotalSeconds > 240)
				{
					needRemoveEntries.Add(pair.Key);
				}
			}

			foreach (var key in needRemoveEntries)
			{
				HttpClientEntry item = _pool[key];
				if (_pool.Remove(key))
				{
					item.Client.Dispose();
				}
			}
		}
	}
}
