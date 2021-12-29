using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetSpider.Infrastructure
{
	public class PrintArgumentService : IHostedService
	{
		private readonly SpiderOptions _options;
		private readonly ILogger<PrintArgumentService> _logger;

		private static readonly string _logo = @"

  _____        _              _    _____       _     _
 |  __ \      | |            | |  / ____|     (_)   | |
 | |  | | ___ | |_ _ __   ___| |_| (___  _ __  _  __| | ___ _ __
 | |  | |/ _ \| __| '_ \ / _ \ __|\___ \| '_ \| |/ _` |/ _ \ '__|
 | |__| | (_) | |_| | | |  __/ |_ ____) | |_) | | (_| |  __/ |
 |_____/ \___/ \__|_| |_|\___|\__|_____/| .__/|_|\__,_|\___|_|     version: {0}
                                        | |
                                        |_|
";

		public PrintArgumentService(IOptions<SpiderOptions> options, ILogger<PrintArgumentService> logger)
		{
			_logger = logger;
			_options = options.Value;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			var properties = typeof(SpiderOptions).GetProperties();
			var version = GetType().Assembly.GetName().Version;
			var versionDescription = version.MinorRevision == 0 ? version.ToString() : $"{version}-beta";
			_logger.LogInformation(string.Format(_logo, versionDescription));
			foreach (var property in properties)
			{
				_logger.LogInformation($"{property.Name}: {property.GetValue(_options)}");
			}

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
