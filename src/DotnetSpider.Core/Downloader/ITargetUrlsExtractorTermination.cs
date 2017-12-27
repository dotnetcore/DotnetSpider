namespace DotnetSpider.Core.Downloader
{
	public interface ITargetUrlsExtractorTermination
	{
		/// <summary>
		/// Return true, skip all urls from target urls builder.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="creator"></param>
		/// <returns></returns>
		bool IsTermination(Page page, TargetUrlsExtractor creator);
	}
}
