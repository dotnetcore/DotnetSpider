using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Downloader;

namespace DotnetSpider.Core.Processor.RequestExtractor
{
	public class XPathRequestExtractor : IRequestExtractor
	{
		private readonly IEnumerable<string> _xPaths;

		public XPathRequestExtractor(params string[] xPaths)
		{
			if (xPaths == null || xPaths.Length == 0) throw new SpiderException($"{nameof(xPaths)} should not be empty.");
			_xPaths = xPaths;
		}

		public XPathRequestExtractor(IEnumerable<string> xPaths) : this(xPaths.ToArray())
		{
		}

		public IEnumerable<Request> Extract(Page page)
		{
			var urls = new List<string>();
			foreach (var xpath in _xPaths)
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
			return _xPaths.Contains(xpath);
		}
	}
}
