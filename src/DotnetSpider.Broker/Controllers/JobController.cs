using Microsoft.Extensions.Logging;

namespace DotnetSpider.Broker.Controllers
{
	public class JobController : BrokerController
	{
		public JobController(ILogger<JobController> logger, BrokerOptions options) : base(logger, options)
		{
		}

		public void Trigger(string jobId)
		{

		}
	}
}
