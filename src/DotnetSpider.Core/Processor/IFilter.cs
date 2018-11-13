using DotnetSpider.Downloader;

namespace DotnetSpider.Core.Processor
{
	public interface IFilter
	{
		bool IsMatch(Request request);
	}
}
