using System.Threading.Tasks;
using DotnetSpider.Http;

namespace DotnetSpider.Agent
{
	public interface IDownloader
	{
		Task<Response> DownloadAsync(Request request);
	}
}
