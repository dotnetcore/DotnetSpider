using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace DotnetSpider.Portal.BackgroundService
{
	public class QuartzService : Microsoft.Extensions.Hosting.BackgroundService
	{
		private readonly IScheduler _sched;

		public QuartzService(IScheduler sched)
		{
			_sched = sched;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return _sched.Start(stoppingToken);
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_sched.Shutdown(cancellationToken);
			return base.StopAsync(cancellationToken);
		}
	}
}
