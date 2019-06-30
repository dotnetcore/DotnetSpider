using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Downloader;
using DotnetSpider.Network;
using DotnetSpider.Network.InternetDetector;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotnetSpider.Tests.Network
{
    public class NetworkCenterTests
    {
        /// <summary>
        /// 测试 NetworkCenter 是否能正常工作
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "NetworkCenter")]
        public Task NetworkCenter()
        {
            var services = new ServiceCollection();
            services.AddSingleton<NetworkCenter>();
            services.ConfigureAppConfiguration(null, null, false);
            services.AddScoped<IDownloaderAgentOptions, DownloaderAgentOptions>();
            services.AddSingleton<IInternetDetector, DefaultInternetDetector>();
            services.AddSingleton<ILockerFactory, FileLockerFactory>();
            services.AddSingleton<IAdslRedialer, DefaultAdslRedialer>();

            var provider = services.BuildServiceProvider();
            var center = provider.GetRequiredService<NetworkCenter>();
            Assert.True(center.SupportAdsl);
            int i = 0;

            Parallel.For(0, 1000, async j =>
            {
                center.Execute(() =>
                {
                    Interlocked.Increment(ref i);
                    if (i % 100 == 0)
                    {
                        center.Redial();
                    }
                });
                await Task.Delay(20);
            });

#if NETFRAMEWORK
            return Framework.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }
    }
}