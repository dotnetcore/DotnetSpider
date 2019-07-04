//using System;
//using System.Threading.Tasks;
//using DotnetSpider.Downloader.Entity;
//using Microsoft.Extensions.Logging;
//
//namespace DotnetSpider.Downloader
//{
//	/// <summary>
//	/// 分配下载器
//	/// </summary>
//	public class DownloaderAllocator : IDownloaderAllocator
//	{
//		private readonly ILoggerFactory _loggerFactory;
//		private readonly IDownloaderAgentOptions _options;
//
//		/// <summary>
//		/// 构造方法
//		/// </summary>
//		/// <param name="options">选项</param>
//		/// <param name="loggerFactory">日志接口工厂</param>
//		public DownloaderAllocator(
//			IDownloaderAgentOptions options,
//			ILoggerFactory loggerFactory)
//		{
//			_loggerFactory = loggerFactory;
//			_options = options;
//		}
//
//		/// <summary>
//		/// 创建下载器
//		/// </summary>
//		/// <param name="agentId">下载器代理标识</param>
//		/// <param name="allotDownloaderMessage">下载器配置信息</param>
//		/// <returns></returns>
//		/// <exception cref="NotImplementedException"></exception>
//		public Task<IDownloader> CreateDownloaderAsync(string agentId,
//			AllocateDownloaderMessage allotDownloaderMessage)
//		{
//			IDownloader downloader = null;
//			switch (allotDownloaderMessage.Type)
//			{
//				case DownloaderType.Empty:
//				{
//					downloader = new EmptyDownloader
//					{
//						AgentId = agentId,
//						Logger = _loggerFactory.CreateLogger<ExceptionDownloader>()
//					};
//					break;
//				}
//				case DownloaderType.Test:
//				{
//					downloader = new TestDownloader
//					{
//						AgentId = agentId,
//						Logger = _loggerFactory.CreateLogger<ExceptionDownloader>()
//					};
//					break;
//				}
//				case DownloaderType.Exception:
//				{
//					downloader = new ExceptionDownloader
//					{
//						AgentId = agentId,
//						Logger = _loggerFactory.CreateLogger<ExceptionDownloader>()
//					};
//					break;
//				}
//				case DownloaderType.WebDriver:
//				{
//					throw new NotImplementedException();
//				}
//				case DownloaderType.HttpClient:
//				{
//					var httpClient = new HttpClientDownloader
//					{
//						AgentId = agentId,
//						UseProxy = allotDownloaderMessage.UseProxy,
//						AllowAutoRedirect = allotDownloaderMessage.AllowAutoRedirect,
//						Timeout = allotDownloaderMessage.Timeout,
//						DecodeHtml = allotDownloaderMessage.DecodeHtml,
//						UseCookies = allotDownloaderMessage.UseCookies,
//						Logger = _loggerFactory.CreateLogger<HttpClientDownloader>(),
//						HttpProxyPool = string.IsNullOrWhiteSpace(_options.ProxySupplyUrl)
//							? null
//							: new HttpProxyPool(new HttpRowTextProxySupplier(_options.ProxySupplyUrl)),
//						RetryTime = allotDownloaderMessage.RetryTimes
//					};
//					httpClient.AddCookies(allotDownloaderMessage.Cookies);
//					downloader = httpClient;
//					break;
//				}
//			}
//
//			return Task.FromResult(downloader);
//		}
//	}
//}