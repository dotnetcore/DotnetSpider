using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Sample
{
	public class DebugAfterDownloaderHandler : AfterDownloadCompleteHandler
	{
		public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
		{
		}
	}
}
