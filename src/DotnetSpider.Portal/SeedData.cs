using System;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Portal.BackgroundService;
using DotnetSpider.Portal.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace DotnetSpider.Portal
{
	public static class SeedData
	{
		public static async Task InitializeAsync(PortalOptions options, IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>()
				.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
			await context.Database.MigrateAsync();
			await context.Database.EnsureCreatedAsync();

			string sql;
			var conn = context.Database.GetDbConnection();
			Stream sqlStream = null;
			switch (options.DatabaseType?.ToLower())
			{
				case "mysql":
				{
					if (await conn.QuerySingleAsync<int>(
						    $"SELECT count(*)  FROM information_schema.TABLES WHERE table_name ='QRTZ_FIRED_TRIGGERS';") ==
					    0
					)
					{
						sqlStream = typeof(SeedData).Assembly
							.GetManifestResourceStream("DotnetSpider.Portal.DDL.MySql.sql");
					}

					break;
				}

				default:
				{
					if (await conn.QuerySingleAsync<int>(
						    $"SELECT COUNT(*) from sysobjects WHERE id = object_id(N'[dbo].[QRTZ_FIRED_TRIGGERS]') AND OBJECTPROPERTY(id, N'') = IsUserTable") ==
					    0
					)
					{
						sqlStream = typeof(SeedData).Assembly
							.GetManifestResourceStream("DotnetSpider.Portal.DDL.SqlServer.sql");
					}

					break;
				}
			}

			if (sqlStream != null)
			{
				using var reader = new StreamReader(sqlStream);
				sql = await reader.ReadToEndAsync();
				await conn.ExecuteAsync(sql);
			}

			var sched = serviceProvider.GetRequiredService<IScheduler>();
			await InitializeSeedDataAsync(context, sched);
		}

		private static async Task InitializeSeedDataAsync(PortalDbContext context, IScheduler sched)
		{
			if (!await context.Spiders.AnyAsync())
			{
				var spider = new Data.Spider
				{
					Name = "cnblogs",
					Cron = "0 1 */1 * * ?",
					Image = "dotnetspider/spiders.startup:latest",
					CreationTime = DateTimeOffset.Now,
					Enabled = true,
					LastModificationTime = DateTimeOffset.Now
				};
				await context.Spiders.AddAsync(spider);
				await context.SaveChangesAsync();

				var trigger = TriggerBuilder.Create().WithCronSchedule(spider.Cron)
					.WithIdentity(spider.Id.ToString())
					.Build();
				var qzJob = JobBuilder.Create<QuartzJob>().WithIdentity(spider.Id.ToString())
					.WithDescription(spider.Name)
					.RequestRecovery(true).Build();
				await sched.ScheduleJob(qzJob, trigger);
			}
		}
	}
}
