using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Core;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
	public class TestDownloader : DownloaderBase
	{
		private static readonly HttpClient HttpClient = new HttpClient();
		private static readonly int _throwExceptionInterval = 8;

		private long _downloadTimes;


		protected override async Task<Response> ImplDownloadAsync(Request request)
		{
			try
			{
				_downloadTimes++;
				if (_downloadTimes % _throwExceptionInterval == 0)
				{
					throw new SpiderException("这是一个测试异常，请忽略");
				}

				var content = await HttpClient.GetStringAsync(request.Url);
				Logger?.LogInformation($"任务 {request.OwnerId} 下载 {request.Url} 成功");
				return new Response
				{
					RawText = content,
					Request = request,
					AgentId = AgentId,
					Success = true
				};
			}
			catch (Exception e)
			{
				Logger?.LogInformation($"任务 {request.OwnerId} 下载 {request.Url} 失败: {e}");
				return new Response
				{
					Request = request,
					AgentId = AgentId,
					Success = false,
					Exception = e.ToString()
				};
			}
		}
	}
}