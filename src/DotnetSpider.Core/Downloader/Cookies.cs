using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Cookie信息配置对象
	/// 大部分情况下从Fiddler中拷贝完整的Cookie字符串, 调用AddCookies方法设置
	/// </summary>
	public class Cookies
	{
		private readonly Dictionary<string, HashSet<Cookie>> _cookies = new Dictionary<string, HashSet<Cookie>>();

		/// <summary>
		/// Cookie的数量
		/// </summary>
		public int Count => _cookies.Count;

		/// <summary>
		/// 获取所有Cookie
		/// </summary>
		/// <returns>Cookies</returns>
		public IReadOnlyDictionary<string, HashSet<Cookie>> GetCookies() => new ReadOnlyDictionary<string, HashSet<Cookie>>(_cookies);

		/// <summary>
		/// 获取所有Cookie
		/// </summary>
		/// <returns>Cookies</returns>
		public IReadOnlyCollection<Cookie> GetCookies(string domain)
		{
			return _cookies.ContainsKey(domain) ? new ReadOnlyCollection<Cookie>(_cookies[domain].ToList()) : new ReadOnlyCollection<Cookie>(new Cookie[0]);
		}

		/// <summary>
		/// 添加Cookies
		/// </summary>
		/// <param name="cookiesStr">Cookies的键值对字符串, 如: a1=b;a2=c;</param>
		/// <param name="domain">作用域</param>
		/// <param name="path">作用路径</param>
		public void AddCookies(string cookiesStr, string domain, string path = "/")
		{
			if (string.IsNullOrEmpty(cookiesStr) || string.IsNullOrWhiteSpace(cookiesStr))
			{
				throw new SpiderException("cookiesStr should not be null or empty.");
			}
			if (string.IsNullOrEmpty(domain) || string.IsNullOrWhiteSpace(domain))
			{
				throw new SpiderException("domain should not be null or empty.");
			}
			if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
			{
				throw new SpiderException("path should not be null or empty.");
			}
			if (!_cookies.ContainsKey(domain))
			{
				_cookies.Add(domain, new HashSet<Cookie>());
			}
			var cookies = cookiesStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var cookie in cookies)
			{
				var keyValue = cookie.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				if (keyValue.Length == 2)
				{
					_cookies[domain].Add(new Cookie(keyValue[0], keyValue[1], domain, path));
				}
				else if (keyValue.Length == 1)
				{
					_cookies[domain].Add(new Cookie(keyValue[0], string.Empty, domain, path));
				}
			}
		}

		/// <summary>
		/// 添加Cookies
		/// </summary>
		/// <param name="cookies">Cookies的键值对</param>
		/// <param name="domain">作用域</param>
		/// <param name="path">作用路径</param>
		public void AddCookies(IDictionary<string, string> cookies, string domain, string path = "/")
		{
			if (string.IsNullOrEmpty(domain) || string.IsNullOrWhiteSpace(domain))
			{
				throw new SpiderException("domain should not be null or empty.");
			}
			if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
			{
				throw new SpiderException("path should not be null or empty.");
			}
			if (cookies == null)
			{
				throw new SpiderException("cookies should not be null.");
			}
			if (!_cookies.ContainsKey(domain))
			{
				_cookies.Add(domain, new HashSet<Cookie>());
			}
			foreach (var cookie in cookies)
			{
				if (string.IsNullOrEmpty(cookie.Key) || string.IsNullOrWhiteSpace(cookie.Key))
				{
					throw new SpiderException("cookie name should not be null or empty.");
				}
				_cookies[domain].Add(new Cookie(cookie.Key, cookie.Value, domain, path));
			}
		}

		/// <summary>
		/// 添加Cookie
		/// </summary>
		/// <param name="cookie">Cookie</param>
		public void AddCookie(Cookie cookie)
		{
			if (cookie != null)
			{
				if (!_cookies.ContainsKey(cookie.Domain))
				{
					_cookies.Add(cookie.Domain, new HashSet<Cookie>());
				}
				_cookies[cookie.Domain].Add(cookie);
			}
		}
	}

	public class Cookie
	{
		public string Name { get; private set; }
		public string Value { get; private set; }
		public string Path { get; private set; }
		public string Domain { get; private set; }

		public Cookie(string name, string value, string domain, string path = "/")
		{
			Name = name;
			Value = value;
			Domain = domain;
			Path = path;
		}

		/// <summary>
		/// <see cref="GetHashCode"/>
		/// </summary>
		/// <returns>哈希值</returns>
		public override int GetHashCode()
		{
			return $"{Name}{Path}{Domain}".GetHashCode();
		}
	}
}
