using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Cookie信息配置对象
	/// 大部分情况下从Fiddler中拷贝完整的Cookie字符串, 调用AddCookies方法设置
	/// </summary>
	public class Cookies
	{
		private readonly Dictionary<string, Dictionary<string, Cookie>> _cookies = new Dictionary<string, Dictionary<string, Cookie>>();

		/// <summary>
		/// Cookie的数量
		/// </summary>
		public int Count => _cookies.Count;

		/// <summary>
		/// 获取所有Cookie
		/// </summary>
		/// <returns>Cookies</returns>
		public IEnumerable<Cookie> GetCookies()
		{
			List<Cookie> cookies = new List<Cookie>();
			foreach (var pair in _cookies)
			{
				cookies.AddRange(pair.Value.Values);
			}
			return cookies;
		}

		/// <summary>
		/// 获取所有Cookie
		/// </summary>
		/// <returns>Cookies</returns>
		public IReadOnlyCollection<Cookie> GetCookies(string domain)
		{
			return _cookies.ContainsKey(domain) ? new ReadOnlyCollection<Cookie>(_cookies[domain].Values.ToList()) : new ReadOnlyCollection<Cookie>(new Cookie[0]);
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
			var cookies = cookiesStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var pair in cookies)
			{
				var keyValue = pair.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				var name = keyValue[0];

				if (keyValue.Length == 2)
				{
					var value = keyValue[1];
					var cookie = new Cookie(name, value, domain, path);
					AddCookie(cookie);
				}
				else if (keyValue.Length == 1)
				{
					var cookie = new Cookie(name, string.Empty, domain, path);
					AddCookie(cookie);
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
			if (cookies == null)
			{
				throw new SpiderException("cookies should not be null.");
			}
			foreach (var pair in cookies)
			{
				var name = pair.Key;
				var value = pair.Value;
				var cookie = new Cookie(name, value, domain, path);
				AddCookie(cookie);
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
				var domain = cookie.Domain;
				if (!_cookies.ContainsKey(domain))
				{
					_cookies.Add(domain, new Dictionary<string, Cookie>());
				}

				if (_cookies[domain].ContainsKey(cookie.Name))
				{
					_cookies[domain][cookie.Name] = cookie;
				}
				else
				{
					_cookies[domain].Add(cookie.Name, cookie);
				}
			}
		}

		/// <summary>
		/// 添加Cookie
		/// </summary>
		/// <param name="name">名称</param>
		/// <param name="value">值</param>
		/// <param name="domain">作用域</param>
		/// <param name="path">作用路径</param>
		public void AddCookie(string name, string value, string domain, string path = "/")
		{
			AddCookie(new Cookie(name, value, domain, path));
		}
	}

	/// <summary>
	/// Cookie 键值对
	/// </summary>
	public class Cookie
	{
		/// <summary>
		/// Cookie 名称
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Cookie 值
		/// </summary>
		public string Value { get; private set; }

		/// <summary>
		/// Cookie 作用路径
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// Cookie 作用域
		/// </summary>
		public string Domain { get; private set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="name">名称</param>
		/// <param name="value">值</param>
		/// <param name="domain">作用域</param>
		/// <param name="path">作用路径</param>
		public Cookie(string name, string value, string domain, string path = "/")
		{
			if (string.IsNullOrEmpty(domain) || string.IsNullOrWhiteSpace(domain))
			{
				throw new SpiderException("domain should not be null or empty.");
			}
			if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
			{
				throw new SpiderException("path should not be null or empty.");
			}
			if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
			{
				throw new SpiderException("cookie name should not be null or empty.");
			}
			Name = name.Trim();
			Value = value?.Trim();
			Domain = domain.Trim();
			Path = path.Trim();
		}
	}
}
