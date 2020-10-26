using System;
using DotnetSpider.Downloader;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider
{
	public static class ServiceCollectionExtensions
	{


		// ReSharper disable once InconsistentNaming
		public static Builder UseMD5HashAlgorithmService(this Builder builder)
		{
			builder.ConfigureServices(x =>
			{
				x.AddSingleton<IHashAlgorithmService, MD5HashAlgorithmService>();
			});
			return builder;
		}
		public static Builder UseRequestHasher(this Builder builder)
		{
			builder.ConfigureServices(x =>
			{
				x.AddSingleton<IRequestHasher, RequestHasher>();
			});
			return builder;
		}
		public static Builder UseRequestHasher<TRequestHasher>(this Builder builder)where TRequestHasher : class, IRequestHasher
		{
			builder.ConfigureServices(x =>
			{
				x.AddSingleton<IRequestHasher, TRequestHasher>();
			});
			return builder;
		}
		public static Builder IgnoreServerCertificateError(this Builder builder)
		{
			builder.Properties[Const.IgnoreSslError] = "true";
			return builder;
		}
		
		/// <summary>
		/// 使用 ADSL 拨号服务
		/// </summary>
		/// <param name="serviceCollection"></param>
		/// <param name="configure"></param>
		/// <returns></returns>
		public static IServiceCollection AddPPPoE(this IServiceCollection serviceCollection,
			Action<PPPoEOptions> configure)
		{
			serviceCollection.TryAddSingleton<PPPoEService>();
			if (configure != null)
			{
				serviceCollection.Configure(configure);
			}

			return serviceCollection;
		}

		public static IHostBuilder UseDockerLifetime(this IHostBuilder builder)
		{
			builder.ConfigureServices(x => { x.AddSingleton<IHostLifetime, DockerLifeTime>(); });
			return builder;
		}
	}
}
