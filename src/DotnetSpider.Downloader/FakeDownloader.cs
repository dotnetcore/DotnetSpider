using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using DotnetSpider.Common;

namespace DotnetSpider.Downloader
{
	public class FakeDownloader : Downloader
	{
		private readonly string _content;

		public FakeDownloader(string content)
		{
			_content = content;
		}

		protected override Response DowloadContent(Request request)
		{
			var response = new Response(request);
			response.Content = _content;
			DetectContentType(response, null);
			return response;
		}
	}
}
