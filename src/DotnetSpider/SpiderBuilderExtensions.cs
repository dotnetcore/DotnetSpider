using System;
using DotnetSpider.Core;
using DotnetSpider.Data;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DotnetSpider
{
	public static class SpiderServiceExtensions
	{
		/// <summary>
		/// 使用 Serilog 日志
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="configure"></param>
		/// <returns></returns>
		public static SpiderBuilder AddSerilog(this SpiderBuilder builder, LoggerConfiguration configure = null)
		{
			builder.Services.AddSerilog(configure);
			return builder;
		}

		public static SpiderBuilder ConfigureAppConfiguration(this SpiderBuilder builder,
			string config = null,
			string[] args = null, bool loadCommandLine = true)
		{
			builder.Services.ConfigureAppConfiguration(config, args, loadCommandLine);
			return builder;
		}

		public static SpiderBuilder ConfigureAppConfiguration(this SpiderBuilder builder,
			IConfigurationRoot configuration)
		{
			builder.Services.AddSingleton<IConfiguration>(configuration);
			return builder;
		}

		/// <summary>
		/// 单机模式
		/// 在单机模式下，使用内存型消息队列，因此只有在此作用域 SpiderBuilder 下构建的的爬虫才会共用一个消息队列。
		/// </summary>
		/// <param name="builder">爬虫构造器</param>
		/// <returns>爬虫构造器</returns>
		public static SpiderBuilder UseStandalone(this SpiderBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			var dotnetSpiderBuilder = new DotnetSpiderBuilder(builder.Services);
			dotnetSpiderBuilder.UseLocalMessageQueue();
			dotnetSpiderBuilder.AddLocalDownloaderAgent(x =>
			{
				x.UseFileLocker();
				x.UseDefaultAdslRedialer();
				x.UseDefaultInternetDetector();
			});
			dotnetSpiderBuilder.AddLocalDownloadCenter();
			dotnetSpiderBuilder.AddSpiderStatisticsCenter(x => x.UseMemory());

			return builder;
		}

		public static SpiderBuilder UseLocalMessageQueue(this SpiderBuilder builder)
		{
			builder.Services.AddSingleton<IMessageQueue, LocalMessageQueue>();
			return builder;
		}

		public static SpiderBuilder AddSpider<T>(this SpiderBuilder builder) where T : Spider
		{
			builder.Services.AddTransient(typeof(T));
			return builder;
		}

		public static SpiderBuilder AddSpider(this SpiderBuilder builder, Type type)
		{
			Check.NotNull(builder, nameof(builder));
			Check.NotNull(type, nameof(type));
			if (!typeof(Spider).IsAssignableFrom(type))
			{
				throw new SpiderException("不能添加非爬虫类型");
			}

			builder.Services.AddTransient(type);
			return builder;
		}
	}
}