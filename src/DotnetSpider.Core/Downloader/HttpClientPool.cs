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
	/// HttpClient池
	/// </summary>
	public class HttpClientPool : IHttpClientPool
	{
		private ulong _getHttpClientCount;
		private readonly ConcurrentDictionary<int, HttpClientItem> _pool = new ConcurrentDictionary<int, HttpClientItem>();
		private HttpClientItem _defaultHttpClientItem;

		/// <summary>
		/// 通过不同的Hash分组, 返回对应的HttpClient
		/// 设计初衷: 某些网站会对COOKIE某部分做承上启下的检测, 因此必须保证: www.a.com/keyword=xxxx&amp;page=1 www.a.com/keyword=xxxx&amp;page=2 在同一个HttpClient里访问
		/// </summary>
		/// <param name="hashCode">分组的哈希</param>
		/// <param name="cookies">Cookies</param>
		/// <returns>HttpClient对象</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public HttpClientItem GetHttpClient(int? hashCode = null, CookieContainer cookies = null)
		{
			if (hashCode == null)
			{
				return _defaultHttpClientItem ?? (_defaultHttpClientItem = CreateDefaultHttpClient(cookies));
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
				var item = CreateDefaultHttpClient(cookies);
				_pool.TryAdd(hashCode.Value, item);
				return item;
			}
		}

		/// <summary>
		/// 设置 Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddCookie(Cookie cookie)
		{
			if (_defaultHttpClientItem != null)
			{
				_defaultHttpClientItem.Handler.CookieContainer.Add(cookie);
			}

			foreach (var item in _pool.Values)
			{
				item.Handler.CookieContainer.Add(cookie);
			}
		}

		private HttpClientItem CreateDefaultHttpClient(CookieContainer cookies = null)
		{
			var handler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
				UseProxy = true,
				UseCookies = true,
				AllowAutoRedirect = true,
				MaxAutomaticRedirections = 10
			};
			handler.CookieContainer = CopyCookieContainer(cookies);
			return new HttpClientItem
			{
				Handler = handler,
				Client = new HttpClient(handler),
				LastUsedTime = DateTime.Now
			};
		}

		private CookieContainer CopyCookieContainer(CookieContainer cookies = null)
		{
			if (cookies == null)
			{
				return new CookieContainer();
			}
			else
			{
				using (MemoryStream stream = new MemoryStream())
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, cookies);
					stream.Seek(0, SeekOrigin.Begin);
					return (CookieContainer)formatter.Deserialize(stream);
				}
			}
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
