using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Java2Dotnet.Spider.Core.Utils
{
	/// <summary>
	/// url and html utils.
	/// </summary>
	public class UrlUtils
	{
		private static readonly Regex PatternForCharset = new Regex("charset\\s*=\\s*['\"]*([^\\s;'\"]*)");

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

				// workaround: java resolves '//path/file + ?foo' to '//path/?foo', not '//path/file?foo' as desired
				//if (url.StartsWith("?"))
				//	url = bas.PathAndQuery + url;

				Uri abs = new Uri(bas, url);

				return abs.AbsoluteUri;
			}
			catch (Exception)
			{
				return url;
			}
		}

		//public static string getHost(string url)
		//{
		//	string host = url;
		//	int i = StringUtils.ordinalIndexOf(url, "/", 3);
		//	if (i > 0)
		//	{
		//		host = StringUtils.substring(url, 0, i);
		//	}
		//	return host;
		//}

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
			return urls.Select(url => new Request(url, grade, null)).ToList();
		}

		public static IList<string> ConvertToUrls(IEnumerable<Request> requests)
		{
			return requests.Select(request => request.Url.ToString()).ToList();
		}

		public static Encoding GetEncoding(string contentType)
		{
			Match match = PatternForCharset.Match(contentType);

			if (!string.IsNullOrEmpty(match.Value))
			{
				string charset = match.Value.Replace("charset=", "");
				try
				{
					return Encoding.GetEncoding(charset);
				}
				catch
				{
					return null;
				}
			}

			return null;
		}
	}
}