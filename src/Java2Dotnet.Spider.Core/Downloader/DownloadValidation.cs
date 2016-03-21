namespace Java2Dotnet.Spider.Core.Downloader
{
	public enum DownloadValidationResult
	{
		Success,
		FailedAndNeedRedial,
		Failed,
		FailedAndNeedUpdateCookie,
		Miss
	}

	public delegate DownloadValidationResult DownloadValidation(Page page);
}
