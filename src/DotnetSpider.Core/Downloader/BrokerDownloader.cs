using DotnetSpider.Common;
using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DotnetSpider.Core.Downloader
{
	public class BrokerDownloader : IDownloader
	{
		public ILogger Logger { get; set; }
		public ICookieInjector CookieInjector { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public void AddAfterDownloadCompleteHandler(IAfterDownloadCompleteHandler handler)
		{
			throw new NotImplementedException();
		}

		public void AddBeforeDownloadHandler(IBeforeDownloadHandler handler)
		{
			throw new NotImplementedException();
		}

		public void AddCookie(Cookie cookie)
		{
			throw new NotImplementedException();
		}

		public void AddCookie(string name, string value, string domain, string path = "/")
		{
			throw new NotImplementedException();
		}

		public void AddCookies(IDictionary<string, string> cookies, string domain, string path = "/")
		{
			throw new NotImplementedException();
		}

		public void AddCookies(string cookiesStr, string domain, string path = "/")
		{
			throw new NotImplementedException();
		}

		public IDownloader Clone()
		{
			return MemberwiseClone() as IDownloader;
		}

		public void Dispose()
		{
		}

		public Response Download(Request request)
		{
			return new Response(request)
			{
				Content = request.Properties["BROKER_CONTENT"],
				StatusCode = request.Properties["BROKER_STATUS"],
				TargetUrl = request.Properties["BROKER_TARGETURL"]
			};
		}
	}
}
