using System;
using System.Threading.Tasks;
using DotnetSpider.Portal.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DotnetSpider.Portal
{
	public class TriggerJob : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			var jobId = context.JobDetail.Key.Name;
			var services = Ioc.ServiceProvider.CreateScope().ServiceProvider;
			var logger = services.GetRequiredService<ILogger<TriggerJob>>();
			try
			{
				var options = services.GetRequiredService<PortalOptions>();
				var dbContext = services.GetRequiredService<PortalDbContext>();
				await JobHelper.RunAsync(options, dbContext, int.Parse(jobId));
			}
			catch (Exception ex)
			{
				logger.LogError($"触发任务 {jobId} 失败: {ex}");
			}
		}
	}
}