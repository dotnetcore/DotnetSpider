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
	/// Httpclient pool impletion for <see cref="IHttpClientPool"/>
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// HttpClient池
	/// </summary>
	public class HttpClientPool : IHttpClientPool
	{
		private ulong _getHttpClientCount;
		private readonly ConcurrentDictionary<int, HttpClientElement> _pool = new ConcurrentDictionary<int, HttpClientElement>();
		private HttpClientElement _defaultHttpClientItem;
		private Dictionary<string, CookieContainer> _initedCookieContainers = new Dictionary<string, CookieContainer>();

		/// <summary>
		/// Get a <see cref="HttpClientElement"/> from <see cref="IHttpClientPool"/>.
		/// Return same <see cref="HttpClientElement"/> instance when <paramref name="hashCode"/> is same.
		/// This can ensure some pages have same CookieContainer.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 通过不同的Hash分组, 返回对应的HttpClient
		/// 设计初衷: 某些网站会对COOKIE某部分做承上启下的检测, 因此必须保证: www.a.com/keyword=xxxx&amp;page=1 www.a.com/keyword=xxxx&amp;page=2 在同一个HttpClient里访问
		/// </summary>
		/// <param name="spider">爬虫 <see cref="ISpider"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		/// <param name="cookieContainer">Cookie <see cref="CookieContainer"/></param>
		/// <param name="hashCode">分组的哈希 Hashcode to identify different group.</param>
		/// <param name="cookieInjector">Cookie注入器 <see cref="ICookieInjector"/></param>
		/// <returns>HttpClientItem</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public HttpClientElement GetHttpClient(ISpider spider, IDownloader downloader, CookieContainer cookieContainer, int? hashCode = null, ICookieInjector cookieInjector = null)
		{
			if (cookieContainer == null)
			{
				throw new SpiderException($"{nameof(cookieContainer)} should not be null");
			}
			if (downloader == null)
			{
				throw new SpiderException($"{nameof(downloader)} should not be null");
			}
			var newCookieContainer = GenerateNewCookieContainer(spider, downloader, cookieContainer, cookieInjector);

			if (hashCode == null)
			{
				return _defaultHttpClientItem ?? (_defaultHttpClientItem = CreateDefaultHttpClient(newCookieContainer));
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
				var item = CreateDefaultHttpClient(newCookieContainer);
				_pool.TryAdd(hashCode.Value, item);
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
			if (_defaultHttpClientItem != null)
			{
				_defaultHttpClientItem.Handler.CookieContainer.Add(cookie);
			}

			foreach (var item in _pool.Values)
			{
				item.Handler.CookieContainer.Add(cookie);
			}
		}

		private CookieContainer GenerateNewCookieContainer(ISpider spider, IDownloader downloader, CookieContainer cookieContainer, ICookieInjector cookieInjector = null)
		{
			var key = $"{cookieContainer.GetHashCode()}_{cookieInjector?.GetHashCode()}";

			if (!_initedCookieContainers.ContainsKey(key))
			{
				cookieInjector?.Inject(downloader, spider);
				// 此处完成COPY一个新的Container的原因是, 某此网站会在COOKIE中设置值, 上下访问有承向启下的关系, 所以必须独立的CookieContainer来管理
				var newCookieContainer = CopyCookieContainer(cookieContainer);
				_initedCookieContainers.Add(key, newCookieContainer);
			}
			return _initedCookieContainers[key];
		}

		private HttpClientElement CreateDefaultHttpClient(CookieContainer cookieContainer)
		{
			var handler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
				UseProxy = true,
				UseCookies = true,
				AllowAutoRedirect = true,
				MaxAutomaticRedirections = 10
			};
			handler.CookieContainer = cookieContainer;

			return new HttpClientElement
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
				HttpClientElement item;
				if (_pool.TryRemove(key, out item))
				{
					item.Client.Dispose();
				}
			}
		}
	}
}
