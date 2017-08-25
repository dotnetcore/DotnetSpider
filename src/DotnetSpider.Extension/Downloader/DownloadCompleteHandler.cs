using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using NLog;

namespace DotnetSpider.Extension.Downloader
{
	public class RedialWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		public string Content { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page.Content) && page.Content.Contains(Content))
			{
				if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
				{
					Logger.MyLog(spider.Identity, "Exit program because redial failed.", LogLevel.Error);
					spider.Exit();
				}
				page = Spider.AddToCycleRetry(page.Request, spider.Site);
				page.Exception = new DownloadException($"Content downloaded contains string: {Content}.");
			}
			return true;
		}
	}

	public class RedialWhenExceptionThrowHandler : AfterDownloadCompleteHandler
	{
		public string ExceptionMessage { get; set; } = string.Empty;

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (page.Exception != null)
			{
				if (string.IsNullOrEmpty(ExceptionMessage))
				{
					page.Exception = new SpiderException("ExceptionMessage should not be empty/null.");
				}
				if (page.Exception.Message.Contains(ExceptionMessage))
				{
					if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
					{
						Logger.MyLog(spider.Identity, "Exit program because redial failed.", LogLevel.Error);
						spider.Exit();
					}
					Spider.AddToCycleRetry(page.Request, spider.Site);
					page.Exception = new DownloadException("Download failed and redial finished already.");
				}
			}
			return true;
		}
	}

	public class RedialAndUpdateCookieWhenContainsContentHandler : AfterDownloadCompleteHandler
	{
		public string Content { get; set; }

		public ICookieInjector CookieInjector { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page.Content) && CookieInjector != null && page.Content.Contains(Content))
			{
				if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
				{
					spider.Exit();
				}
				Spider.AddToCycleRetry(page.Request, spider.Site);
				CookieInjector?.Inject(spider);
				page.Exception = new DownloadException($"Content downloaded contains string: {Content}.");
			}
			return true;
		}
	}

	public class CycleRedialHandler : AfterDownloadCompleteHandler
	{
		public int RedialLimit { get; set; }

		public static int RequestedCount { get; set; }

		public override bool Handle(ref Page page, ISpider spider)
		{
			if (RedialLimit != 0)
			{
				lock (this)
				{
					++RequestedCount;

					if (RedialLimit > 0 && RequestedCount == RedialLimit)
					{
						RequestedCount = 0;
						Spider.AddToCycleRetry(page.Request, spider.Site);
						if (NetworkCenter.Current.Executor.Redial() == RedialResult.Failed)
						{
							spider.Exit();
						}
					}
				}
			}
			return true;
		}
	}
}