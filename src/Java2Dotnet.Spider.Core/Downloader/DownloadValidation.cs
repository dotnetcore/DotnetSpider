namespace Java2Dotnet.Spider.Core.Downloader
{
	public enum DownloadValidationResult
	{
		Success,
		FailedAndNeedRedial,
		Failed,
		FailedAndNeedUpdateCookie,
		Miss,
		FailedAndNeedRetryOrWait
	}

	public delegate DownloadValidationResult DownloadValidation(Page page);
}
