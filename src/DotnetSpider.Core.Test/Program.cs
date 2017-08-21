#if !NET_CORE
using System.IO;
using DotnetSpider.Core.Downloader;
using static DotnetSpider.Core.Test.SpiderTest;

namespace DotnetSpider.Core.Test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var path = "www.baidu.com.cookies";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.WriteAllText(path, "a=b&c=d");

			Spider spider = Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 }, new TestPageProcessor()).AddPipeline(new TestPipeline());
			spider.ThreadNum = 1;
			var downloader = new HttpClientDownloader();
			downloader.AddAfterDownloadCompleteHandler(new TimerUpdateCookieHandler(5, new FileCookieInject()));
			spider.Downloader = downloader;

			for (int i = 0; i < 10000; i++)
			{
				spider.AddStartUrl("http://www.baidu.com/" + i);
			}
			spider.Run();
		}
	}
}
#endif