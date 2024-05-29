using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider.Downloader;

public static class ServiceCollectionExtensions
{
    // /// <summary>
    // /// 只有本地爬虫才能配置下载器，分布式爬虫的下载器注册是在下载器代理中
    // /// </summary>
    // /// <param name="builder"></param>
    // /// <typeparam name="TDownloader"></typeparam>
    // /// <returns></returns>
    // public static Builder UseDownloader<TDownloader>(this Builder builder)
    //     where TDownloader : class, IDownloader
    // {
    //     builder.ConfigureServices(x =>
    //     {
    //         x.AddTransient<HttpMessageHandlerBuilder, DefaultHttpMessageHandlerBuilder>();
    //         x.AddKeyedSingleton<IDownloader>(typeof(TDownloader).Name);
    //         x.AddAgentHostService(opts =>
    //         {
    //             opts.AgentId = ObjectId.CreateId().ToString();
    //             opts.AgentName = opts.AgentId;
    //         });
    //     });
    //
    //     return builder;
    // }

    public static Builder AddDownloader<TDownloader>(this Builder builder)
        where TDownloader : class, IDownloader
    {
        builder.ConfigureServices(x =>
        {
            x.AddKeyedSingleton<IDownloader>(typeof(TDownloader).Name);
        });

        return builder;
    }

    public static IServiceCollection AddDownloader<TDownloader>(this IServiceCollection services)
        where TDownloader : class, IDownloader
    {
        var name = typeof(TDownloader).Name;
        services.AddKeyedSingleton<IDownloader, TDownloader>(name);
        return services;
    }
}
