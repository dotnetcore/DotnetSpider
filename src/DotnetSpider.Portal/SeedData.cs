using System;
using System.IO;
using System.Linq;
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
			using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>()
				.CreateScope())
			{
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

				if (sqlStream == null)
				{
					throw new SpiderException("Can't find quartz.net MySql sql");
				}

				using (var reader = new StreamReader(sqlStream))
				{
					sql = await reader.ReadToEndAsync();
					await conn.ExecuteAsync(sql);
				}

				var sched = serviceProvider.GetRequiredService<IScheduler>();
				await InitializeSeedDataAsync(context, sched);
			}
		}

		private static async Task InitializeSeedDataAsync(PortalDbContext context, IScheduler sched)
		{
			if (!await context.DockerRepositories.AnyAsync())
			{
				var repo = new DockerRepository
				{
					Name = "DockerHub",
					Schema = null,
					Registry = null,
					Repository = "dotnetspider/spiders.startup",
					CreationTime = DateTimeOffset.Now,
					UserName = "",
					Password = ""
				};
				await context.DockerRepositories.AddAsync(repo);

				var spider = new Data.Spider
				{
					Name = "cnblogs",
					Cron = "0 1 */1 * * ?",
					Repository = "dotnetspider/spiders.startup",
					Type = "DotnetSpider.Spiders.CnblogsSpider",
					Tag = "latest",
					CreationTime = DateTimeOffset.Now
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
