using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// url and html utils.
	/// </summary>
	public static class UrlUtils
	{
		public static string GetBaseDomain(string url)
		{
			var host = new Uri(url).Host;
			List<string> list = new List<string>(".com|.co|.info|.net|.org|.me|.mobi|.us|.biz|.xxx|.ca|.co.jp|.com.cn|.net.cn|.org.cn|.mx|.tv|.ws|.ag|.com.ag|.net.ag|.org.ag|.am|.asia|.at|.be|.com.br|.net.br|.bz|.com.bz|.net.bz|.cc|.com.co|.net.co|.nom.co|.de|.es|.com.es|.nom.es|.org.es|.eu|.fm|.fr|.gs|.in|.co.in|.firm.in|.gen.in|.ind.in|.net.in|.org.in|.it|.jobs|.jp|.ms|.com.mx|.nl|.nu|.co.nz|.net.nz|.org.nz|.se|.tc|.tk|.tw|.com.tw|.idv.tw|.org.tw|.hk|.co.uk|.me.uk|.org.uk|.vg".Split('|'));
			string[] hs = host.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

			if (hs.Length > 2)
			{
				//�����host��ַ����������
				int p2 = host.LastIndexOf('.');                 //���һ�Ρ�.�����ֵ�λ��
				int p1 = host.Substring(0, p2).LastIndexOf('.');//�����ڶ�����.�����ֵ�λ��
				string s1 = host.Substring(p1);
				if (!list.Contains(s1))
					return s1.TrimStart('.');

				//������׺Ϊ���Σ����á�.���ָ���
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
		///  
		/// </summary>
		/// <param name="url"></param>
		/// <param name="refer"></param>
		/// <returns></returns>
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

		public static string RemoveProtocol(string url)
		{
			return Regex.Replace(url, "[\\w]+://", "", RegexOptions.IgnoreCase);
		}

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

		public static IList<Request> ConvertToRequests(IEnumerable<string> urls, int grade)
		{
			return urls.Select(url => new Request(url) { Depth = grade }).ToList();
		}

		public static IList<string> ConvertToUrls(IEnumerable<Request> requests)
		{
			return requests.Select(request => request.Url.ToString()).ToList();
		}
	}
}