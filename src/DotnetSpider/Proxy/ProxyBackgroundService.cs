using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetSpider.Proxy;

public class ProxyBackgroundService(
    IProxyService pool,
    ILogger<ProxyService> logger,
    IServiceProvider serviceProvider,
    IOptions<SpiderOptions> options)
    : BackgroundService
{
    private readonly IProxySupplier _proxySupplier =
        serviceProvider.GetService(typeof(IProxySupplier)) as IProxySupplier;

    private readonly ILogger _logger = logger;
    private readonly SpiderOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_proxySupplier == null)
        {
            throw new ArgumentException("None proxy supplier");
        }

        await Task.Factory.StartNew(async () =>
        {
            var interval = _options.RefreshProxy * 1000;
            while (!stoppingToken.IsCancellationRequested)
            {
                var failedNum = 0;
                try
                {
                    var proxies = await _proxySupplier.GetProxiesAsync();
                    var cnt = await pool.AddAsync(proxies);
                    if (cnt > 0)
                    {
                        _logger.LogInformation("Find new {ProxyCount} proxies", cnt);
                    }

                    await Task.Delay(interval, default);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Get proxies failed");
                    failedNum++;
                    if (failedNum > 5)
                    {
                        break;
                    }
                }
            }
        }, stoppingToken);
    }
}
