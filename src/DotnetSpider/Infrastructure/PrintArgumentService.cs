using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static System.String;

namespace DotnetSpider.Infrastructure;

public class PrintArgumentService(IOptions<SpiderOptions> options, ILogger<PrintArgumentService> logger)
    : IHostedService
{
    private readonly SpiderOptions _options = options.Value;

    private const string Info = @"

  _____        _              _    _____       _     _
 |  __ \      | |            | |  / ____|     (_)   | |
 | |  | | ___ | |_ _ __   ___| |_| (___  _ __  _  __| | ___ _ __
 | |  | |/ _ \| __| '_ \ / _ \ __|\___ \| '_ \| |/ _` |/ _ \ '__|
 | |__| | (_) | |_| | | |  __/ |_ ____) | |_) | | (_| |  __/ |
 |_____/ \___/ \__|_| |_|\___|\__|_____/| .__/|_|\__,_|\___|_|     version: {0}
                                        | |
                                        |_|
";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var properties = typeof(SpiderOptions).GetProperties();
        var version = GetType().Assembly.GetName().Version;
        if (version == null)
        {
            throw new ArgumentException("Assembly version is null");
        }

        var versionDescription = version.MinorRevision == 0 ? version.ToString() : $"{version}-beta";
        logger.LogInformation(Format(Info, versionDescription));
        var config = Join(", ", properties.Select(x => $"{x.Name}: {x.GetValue(_options)}"));
        logger.LogInformation(config);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
