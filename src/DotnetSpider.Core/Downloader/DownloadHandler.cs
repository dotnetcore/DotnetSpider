namespace DotnetSpider.Core.Downloader
{
	//public enum DownloadValidationResult
	//{
	//	Success,
	//	FailedAndNeedRedial,
	//	Failed,
	//	FailedAndNeedUpdateCookie,
	//	Miss,
	//	FailedAndNeedRetryOrWait,
	//	FailedAndNeedWaitToVerifyCode
	//}

	public interface IDownloadHandler
	{
		void Handle(Page page);
	}
}
