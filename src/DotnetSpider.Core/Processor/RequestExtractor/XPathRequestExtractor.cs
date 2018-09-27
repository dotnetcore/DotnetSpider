using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Downloader;

namespace DotnetSpider.Core.Processor.RequestExtractor
{
	public class XPathRequestExtractor : IRequestExtractor
	{
		private readonly IEnumerable<string> _xpaths;

		public XPathRequestExtractor(params string[] xpaths)
		{
			if (xpaths == null || xpaths.Length == 0) throw new SpiderException($"{nameof(xpaths)} should not be empty.");
			_xpaths = xpaths;
		}

		public XPathRequestExtractor(IEnumerable<string> xpaths) : this(xpaths.ToArray())
		{
		}

		public IEnumerable<Request> Extract(Page page)
		{
			var urls = new List<string>();
			foreach (var xpath in _xpaths)
			{
				var links = page.Selectable().XPath(xpath).Links().GetValues();
				foreach (var link in links)
				{
#if !NETSTANDARD
					urls.Add(System.Web.HttpUtility.HtmlDecode(System.Web.HttpUtility.UrlDecode(link)));
#else
					urls.Add(System.Net.WebUtility.HtmlDecode(System.Net.WebUtility.UrlDecode(link)));
#endif
				}
			}
			return urls.Select(url => new Request(url, page.CopyProperties()));
		}

		internal bool ContainsXpath(string xpath)
		{
			return _xpaths.Contains(xpath);
		}
	}
}
