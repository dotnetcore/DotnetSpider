using System.Threading.Tasks;
using DotnetSpider.Http;

namespace DotnetSpider.Downloader
{
	public interface IDownloader
	{
		Task<Response> DownloadAsync(Request request);

		string Name { get; }
	}
}
