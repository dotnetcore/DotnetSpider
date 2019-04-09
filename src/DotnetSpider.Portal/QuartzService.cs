using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace DotnetSpider.Portal
{
	public class QuartzService : IHostedService
	{
		private IScheduler _sched;
		private readonly QuartzOptions _options;

		public QuartzService(QuartzOptions options, ILogProvider loggingProvider)
		{
			LogProvider.SetCurrentLogProvider(loggingProvider);
			options.Properties.Add("quartz.jobStore.useProperties", "true");
			_options = options;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			var properties = new NameValueCollection();
			foreach (var property in _options.Properties)
			{
				properties.Add(property.Key, property.Value);
			}

			_sched = await new StdSchedulerFactory(properties).GetScheduler(cancellationToken);
			await _sched.Start(cancellationToken);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _sched.Shutdown(cancellationToken);
			_sched = null;
		}
	}
}