using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// url and html utils.
	/// </summary>
	public class UrlUtils
	{
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
			return urls.Select(url => new Request(url, null) { Depth = grade }).ToList();
		}

		public static IList<string> ConvertToUrls(IEnumerable<Request> requests)
		{
			return requests.Select(request => request.Url.ToString()).ToList();
		}
	}
}