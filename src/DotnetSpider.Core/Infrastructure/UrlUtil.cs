using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// Url 的帮助类
	/// </summary>
	public static class UrlUtil
	{
		/// <summary>
		/// 取得链接的一级域名
		/// </summary>
		/// <param name="url">链接</param>
		/// <returns>一级域名</returns>
		public static string GetBaseDomain(string url)
		{
			var host = new Uri(url).Host;
			List<string> list = new List<string>(".com|.co|.info|.net|.org|.me|.mobi|.us|.biz|.xxx|.ca|.co.jp|.com.cn|.net.cn|.org.cn|.mx|.tv|.ws|.ag|.com.ag|.net.ag|.org.ag|.am|.asia|.at|.be|.com.br|.net.br|.bz|.com.bz|.net.bz|.cc|.com.co|.net.co|.nom.co|.de|.es|.com.es|.nom.es|.org.es|.eu|.fm|.fr|.gs|.in|.co.in|.firm.in|.gen.in|.ind.in|.net.in|.org.in|.it|.jobs|.jp|.ms|.com.mx|.nl|.nu|.co.nz|.net.nz|.org.nz|.se|.tc|.tk|.tw|.com.tw|.idv.tw|.org.tw|.hk|.co.uk|.me.uk|.org.uk|.vg".Split('|'));
			string[] hs = host.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

			if (hs.Length > 2)
			{
				int p2 = host.LastIndexOf('.');
				int p1 = host.Substring(0, p2).LastIndexOf('.');
				string s1 = host.Substring(p1);
				if (!list.Contains(s1))
					return s1.TrimStart('.');


				if (hs.Length > 3)
					return host.Substring(host.Substring(0, p1).LastIndexOf('.'));
				else
					return host.TrimStart('.');
			}
			else if (hs.Length == 2)
			{
				return host.TrimStart('.');
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// 计算最终的URL
		/// </summary>
		/// <param name="url">Base uri</param>
		/// <param name="refer">Relative uri</param>
		/// <returns>最终的URL</returns>
		public static string CanonicalizeUrl(string url, string refer)
		{
			try
			{
				Uri bas = new Uri(refer);
				Uri abs = new Uri(bas, url);
				return abs.AbsoluteUri;
			}
			catch (Exception)
			{
				return url;
			}
		}

		/// <summary>
		/// 去掉URL中的协议
		/// </summary>
		/// <param name="url">URL</param>
		/// <returns>去掉协议后的URL</returns>
		public static string RemoveProtocol(string url)
		{
			return Regex.Replace(url, "[\\w]+://", "", RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// 取得URL的域名
		/// </summary>
		/// <param name="url">链接</param>
		/// <returns>域名</returns>
		public static string GetDomain(string url)
		{
			string domain = RemoveProtocol(url);
			int i = domain.IndexOf("/", 1, StringComparison.Ordinal);
			if (i > 0)
			{
				domain = domain.Substring(0, i);
			}
			return domain;
		}
	}
}