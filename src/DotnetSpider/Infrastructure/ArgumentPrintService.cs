using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetSpider.Infrastructure
{
    public class ArgumentPrintService : IHostedService
    {
        private readonly SpiderOptions _options;
        private readonly ILogger<ArgumentPrintService> _logger;

        public ArgumentPrintService(IOptions<SpiderOptions> options, ILogger<ArgumentPrintService> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var properties = typeof(SpiderOptions).GetProperties();
            foreach (var property in properties)
            {
                _logger.LogInformation($"Argument: {property.Name}, {property.GetValue(_options)}");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}