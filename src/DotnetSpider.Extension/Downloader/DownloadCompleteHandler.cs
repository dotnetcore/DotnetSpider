using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Redial;

namespace DotnetSpider.Extension.Downloader
{
	#region Redial Handler

	public class RedialWhenContainsIllegalStringHandler : DownloadCompleteHandler
	{
		public string ContainsString { get; set; }

		public override void Handle(Page page)
		{
			string rawText = page.Content;
			if (string.IsNullOrEmpty(rawText))
			{
				throw new DownloadException("Download failed or response is null.");
			}
			if (rawText.Contains(ContainsString))
			{
				((IRedialExecutor)NetworkCenter.Current.Executor).Redial();
				throw new DownloadException($"Content downloaded contains illegal string: {ContainsString}.");
			}
		}
	}

	public class RedialWhenExceptionThrowHandler : DownloadCompleteHandler
	{
		public string ExceptionMessage { get; set; } = string.Empty;

		public override void Handle(Page page)
		{
			if (page.Exception != null)
			{
				if (string.IsNullOrEmpty(ExceptionMessage))
				{
					throw new SpiderException("ExceptionMessage should not be empty/null.");
				}
				if (page.Exception.Message.Contains(ExceptionMessage))
				{
					((IRedialExecutor)NetworkCenter.Current.Executor).Redial();
					throw new DownloadException("Download failed and redial finished already.");
				}
			}
		}
	}

	public class CycleRedialHandler : DownloadCompleteHandler
	{
		public int RedialLimit { get; set; }
		public static int RequestedCount { get; set; }

		public override void Handle(Page page)
		{
			if (RedialLimit != 0)
			{
				lock (this)
				{
					++RequestedCount;

					if (RedialLimit > 0 && RequestedCount == RedialLimit)
					{
						RequestedCount = 0;

						((IRedialExecutor)NetworkCenter.Current.Executor).Redial();
					}
				}
			}
		}
	}

	#endregion
}
