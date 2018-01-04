using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// HttpClient池
	/// </summary>
	public class HttpClientPool : IHttpClientPool
	{
		private ulong _getHttpClientCount;
		private readonly ConcurrentDictionary<int, HttpClientItem> _pool = new ConcurrentDictionary<int, HttpClientItem>();
		private HttpClientItem _defaultHttpClientItem;

		/// <summary>
		/// 通过不同的Hash分组, 返回对应的HttpClient
		/// 设计初衷: 某些网站会对COOKIE某部分做承上启下的检测, 因此必须保证: www.a.com/keyword=xxxx&page=1 www.a.com/keyword=xxxx&page=2 在同一个HttpClient里访问
		/// </summary>
		/// <param name="hashCode">分组的哈希</param>
		/// <param name="cookies">Cookies</param>
		/// <returns>HttpClient对象</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public HttpClientItem GetHttpClient(int? hashCode = null, Cookies cookies = null)
		{
			if (hashCode == null)
			{
				if (_defaultHttpClientItem == null)
				{
					_defaultHttpClientItem = CreateDefaultHttpClient(cookies.GetCookies());
				}
				return _defaultHttpClientItem;
			}

			_getHttpClientCount++;

			if (_getHttpClientCount % 100 == 0)
			{
				ClearHttpClient();
			}

			if (_pool.ContainsKey(hashCode.Value))
			{
				_pool[hashCode.Value].LastUsedTime = DateTime.Now;
				return _pool[hashCode.Value];
			}
			else
			{
				var item = CreateDefaultHttpClient(cookies.GetCookies());
				_pool.TryAdd(hashCode.Value, item);
				return item;
			}
		}

		/// <summary>
		/// 重置Cookie
		/// </summary>
		/// <param name="cookies">Cookies</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ResetCookies(Cookies cookies)
		{
			if (_defaultHttpClientItem != null)
			{
				_defaultHttpClientItem.Handler.CookieContainer = CreateCookieContainer(cookies.GetCookies());
			}

			foreach (var item in _pool.Values)
			{
				item.Handler.CookieContainer = CreateCookieContainer(cookies.GetCookies());
			}
		}

		private HttpClientItem CreateDefaultHttpClient(IEnumerable<Cookie> cookies = null)
		{
			var handler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
				UseProxy = true,
				UseCookies = true,
				AllowAutoRedirect = true,
				MaxAutomaticRedirections = 10
			};
			handler.CookieContainer = CreateCookieContainer(cookies);
			return new HttpClientItem
			{
				Handler = handler,
				Client = new HttpClient(handler),
				LastUsedTime = DateTime.Now
			};
		}

		private CookieContainer CreateCookieContainer(IEnumerable<Cookie> cookies = null)
		{
			CookieContainer container = new CookieContainer();
			if (cookies != null && cookies.Count() > 0)
			{
				foreach (var cookie in cookies)
				{
					container.Add(new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path));
				}
			}
			return container;
		}

		private void ClearHttpClient()
		{
			List<int> needRemoveList = new List<int>();
			var now = DateTime.Now;
			foreach (var pair in _pool)
			{
				if ((now - pair.Value.LastUsedTime).TotalSeconds > 240)
				{
					needRemoveList.Add(pair.Key);
				}
			}

			foreach (var key in needRemoveList)
			{
				HttpClientItem item;
				if (_pool.TryRemove(key, out item))
				{
					item.Client.Dispose();
				}
			}
		}
	}
}
