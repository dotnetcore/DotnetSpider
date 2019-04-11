using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace DotnetSpider.Portal
{
	public class QuartzService : IHostedService
	{
		private readonly IScheduler _sched;

		public QuartzService(IScheduler sched)
		{
			_sched = sched;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await _sched.Start(cancellationToken);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _sched.Shutdown(cancellationToken);
		}
	}
}