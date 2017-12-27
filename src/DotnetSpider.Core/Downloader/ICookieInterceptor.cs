namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Cookie注入器
	/// </summary>
	public interface ICookieInjector
	{
		void Inject(ISpider spider, bool stopSpider = true);
	}
}
