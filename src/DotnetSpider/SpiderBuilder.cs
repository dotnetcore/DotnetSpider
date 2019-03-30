using DotnetSpider.Core;
using DotnetSpider.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider
{
    public class SpiderBuilder
    {
        private SpiderProvider _spiderProvider;

        public IServiceCollection Services { get; }

        public SpiderBuilder()
        {
            Services = new ServiceCollection();

            Services.AddSingleton<IStatisticsService, StatisticsService>();
            Services.AddScoped<ISpiderOptions, SpiderOptions>();
            Services.AddScoped<Spider>();
        }

        public SpiderProvider Build(bool start = true)
        {
            if (_spiderProvider == null)
            {
                var serviceProvider = Services.BuildServiceProvider();
                _spiderProvider = new SpiderProvider(serviceProvider);

                if (start)
                {
                    _spiderProvider.Start();
                }
            }

            return _spiderProvider;
        }
    }
}