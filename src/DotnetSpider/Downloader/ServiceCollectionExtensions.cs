using DotnetSpider.Agent;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;

namespace DotnetSpider.Downloader
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// 只有本地爬虫才能配置下载器，分布式爬虫的下载器注册是在下载器代理中
		/// </summary>
		/// <param name="builder"></param>
		/// <typeparam name="TDownloader"></typeparam>
		/// <returns></returns>
		public static Builder UseDownloader<TDownloader>(this Builder builder)
			where TDownloader : class, IDownloader
		{
			builder.ConfigureServices(x =>
			{
				if (!typeof(IProxyDownloader).IsAssignableFrom(typeof(TDownloader)))
				{
					x.AddTransient<HttpMessageHandlerBuilder, DefaultHttpMessageHandlerBuilder>();
				}

				x.AddAgent<TDownloader>(opts =>
				{
					opts.AgentId = ObjectId.NewId().ToString();
					opts.AgentName = opts.AgentId;
				});
			});

			return builder;
		}
	}
}
