using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper;
using DotnetSpider.Common;
using DotnetSpider.Kafka;
using DotnetSpider.MySql;
using DotnetSpider.Portal.Entity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using ServiceProvider = DotnetSpider.Portal.Common.ServiceProvider;

namespace DotnetSpider.Portal
{
	public class Startup
	{
		private readonly PortalOptions _options;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			_options = new PortalOptions(Configuration);
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<SpiderOptions>();
			services.AddSingleton<PortalOptions>();

			services.AddKafka();
			services.AddDownloadCenter(x => x.UseMySql());
			services.AddStatisticsCenter(x => x.UseMySql());

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			// Add DbContext
			Action<DbContextOptionsBuilder> dbContextOptionsBuilder;
			var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
			switch (_options.Database?.ToLower())
			{
				case "mysql":
				{
					dbContextOptionsBuilder = b =>
						b.UseMySql(_options.ConnectionString,
							sql => sql.MigrationsAssembly(migrationsAssembly));
					break;
				}

				default:
				{
					dbContextOptionsBuilder = b =>
						b.UseSqlServer(_options.ConnectionString,
							sql => sql.MigrationsAssembly(migrationsAssembly));
					break;
				}
			}

			services.AddDbContext<PortalDbContext>(dbContextOptionsBuilder);
			services.AddQuartz();
			services.AddHostedService<QuartzService>();
			services.AddHostedService<CleanDockerContainerService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			ServiceProvider.Instance = app.ApplicationServices;

			PrintEnvironment(app.ApplicationServices.GetRequiredService<ILogger<Startup>>());
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				// app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

			using (IServiceScope scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
				.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
				context.Database.Migrate();
				context.Database.EnsureCreated();

				string sql;
				var conn = context.Database.GetDbConnection();
				switch (_options.Database?.ToLower())
				{
					case "mysql":
					{
						if (conn.QuerySingle<int>(
							    "SELECT count(*)  FROM information_schema.TABLES WHERE table_name ='QRTZ_FIRED_TRIGGERS';") ==
						    0
						)
						{
							using (var reader = new StreamReader(GetType().Assembly
								.GetManifestResourceStream("DotnetSpider.Portal.DDL.MySql.sql")))
							{
								sql = reader.ReadToEnd();
								conn.Execute(sql);
							}
						}

						break;
					}

					default:
					{
						if (conn.QuerySingle<int>(
							    "SELECT COUNT(*) from sysobjects WHERE id = object_id(N'[dbo].[QRTZ_FIRED_TRIGGERS]') AND OBJECTPROPERTY(id, N'') = IsUserTable") ==
						    0
						)
						{
							using (var reader = new StreamReader(GetType().Assembly
								.GetManifestResourceStream("DotnetSpider.Portal.DDL.SqlServer.sql")))
							{
								sql = reader.ReadToEnd();
								conn.Execute(sql);
							}
						}

						break;
					}
				}

				var sched = app.ApplicationServices.GetRequiredService<IScheduler>();
				InitializedData(context, sched);
			}
		}

		private void InitializedData(PortalDbContext context, IScheduler sched)
		{
			if (!context.DockerRepositories.Any())
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
				context.DockerRepositories.Add(repo);

				var spider = new Entity.Spider
				{
					Name = "cnblogs",
					Cron = "0 1 */1 * * ?",
					Repository = "dotnetspider/spiders.startup",
					Type = "DotnetSpider.Spiders.CnblogsSpider",
					Tag = "latest",
					CreationTime = DateTimeOffset.Now
				};
				context.Spiders.Add(spider);
				context.SaveChanges();

				var trigger = TriggerBuilder.Create().WithCronSchedule(spider.Cron).WithIdentity(spider.Id.ToString())
					.Build();
				var qzJob = JobBuilder.Create<TriggerJob>().WithIdentity(spider.Id.ToString())
					.WithDescription(spider.Name)
					.RequestRecovery(true).Build();
				sched.ScheduleJob(qzJob, trigger).GetAwaiter().GetResult();
			}
		}

		private void PrintEnvironment(ILogger logger)
		{
			Framework.PrintInfo();
			logger.LogInformation("Arg   : VERSION = 20190725", 0, ConsoleColor.DarkYellow);
			foreach (var kv in Configuration.GetChildren())
			{
				logger.LogInformation($"Arg   : {kv.Key} = {kv.Value}", 0, ConsoleColor.DarkYellow);
			}

			logger.LogInformation($"BaseDirectory   : {AppDomain.CurrentDomain.BaseDirectory}", 0,
				ConsoleColor.DarkYellow);
			logger.LogInformation(
				$"OS    : {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "X64" : "X86")}", 0,
				ConsoleColor.DarkYellow);
		}
	}
}
