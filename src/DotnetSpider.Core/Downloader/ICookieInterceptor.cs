namespace DotnetSpider.Core.Downloader
{
	public interface ICookieInjector
	{
		void Inject(ISpider spider, bool stopSpider = true);
	}
}
