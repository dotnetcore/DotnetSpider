using DotnetSpider.Core;
using Microsoft.Extensions.Hosting;
#if !NET451

#else
using DotnetSpider.Core;
#endif

namespace DotnetSpider.DownloadAgentRegisterCenter
{
	/// <summary>
	/// 下载中心
	/// </summary>
	public interface IDownloadAgentRegisterCenter : IHostedService, IRunnable
	{
	}
}