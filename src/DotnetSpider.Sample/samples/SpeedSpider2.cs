using System;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DotnetSpider.Sample.samples;

/// <summary>
/// https://localhost:5001/WeatherForecast
/// </summary>
public class SpeedSpider2(
    IOptions<SpiderOptions> options,
    DependenceServices services,
    ILogger<Spider> logger)
    : Spider(options, services, logger)
{
    public static async Task RunAsync()
    {
        var builder = Builder.CreateDefaultBuilder<SpeedSpider2>(options =>
        {
            options.Speed = 100;
        });
        builder.UseSerilog();
        builder.IgnoreServerCertificateError();
        await builder.Build().RunAsync();
    }

    protected override async Task InitializeAsync(CancellationToken stoppingToken = default)
    {
        for (var i = 0; i < 100000; ++i)
        {
            await AddRequestsAsync(new Request("https://localhost:5001/WeatherForecast?_v=" + i));
        }

        AddDataFlow<MyDataFlow>();
    }

    protected override SpiderId GenerateSpiderId()
    {
        return new(ObjectId.CreateId().ToString(), "speed");
    }

    protected class MyDataFlow : DataFlowBase
    {
        private int _downloadCount;

        private DateTime _start;

        public override Task InitializeAsync()
        {
            _start = DateTime.Now;
            return Task.CompletedTask;
        }

        public override async Task HandleAsync(DataFlowContext context, ResponseDelegate next)
        {
            Interlocked.Increment(ref _downloadCount);
            if ((_downloadCount % 100) == 0)
            {
                var sec = (DateTime.Now - _start).TotalSeconds;
                var speed = (decimal)(_downloadCount / sec);
                Logger.LogInformation($"Speed {decimal.Round(speed, 2)}");
            }

            await next(context);
        }
    }
}
