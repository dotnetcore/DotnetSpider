using System;
using System.Reflection;
using DotnetSpider.AgentCenter;
using DotnetSpider.MySql.AgentCenter;
using DotnetSpider.Portal.BackgroundService;
using DotnetSpider.Portal.Common;
using DotnetSpider.Portal.Data;
using DotnetSpider.Portal.ViewObject;
using DotnetSpider.RabbitMQ;
using DotnetSpider.Statistics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz.AspNetCore;
using Quartz.AspNetCore.MySqlConnector;
using ServiceProvider = DotnetSpider.Portal.Common.ServiceProvider;

namespace DotnetSpider.Portal
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.TryAddSingleton<PortalOptions>();

			services.AddControllersWithViews()
				.SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
				.AddNewtonsoftJson()
				.AddRazorRuntimeCompilation();
			services.AddHealthChecks();

			var options = new PortalOptions(Configuration);

			// Add DbContext
			Action<DbContextOptionsBuilder> dbContextOptionsBuilder;
			var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
			switch (options.DatabaseType?.ToLower())
			{
				case "mysql":
				{
					dbContextOptionsBuilder = b =>
						b.UseMySql(ServerVersion.AutoDetect(options.ConnectionString),
							sql => sql.MigrationsAssembly(migrationsAssembly));
					break;
				}

				default:
				{
					dbContextOptionsBuilder = b =>
						b.UseSqlServer(options.ConnectionString,
							sql => sql.MigrationsAssembly(migrationsAssembly));
					break;
				}
			}


			services.AddDbContext<PortalDbContext>(dbContextOptionsBuilder);
			services.AddQuartz(x =>
			{
				x.UseMySqlConnector(options.ConnectionString);
			});
			services.Configure<AgentCenterOptions>(Configuration);
			services.AddHttpClient();
			services.AddAgentCenter<MySqlAgentStore>();
			services.AddStatistics<MySqlStatisticsStore>();
			services.AddRabbitMQ(Configuration);
			services.AddHostedService<QuartzService>();
			services.AddHostedService<CleanDockerContainerService>();
			services.AddSingleton<IActionResultTypeMapper, ActionResultTypeMapper>();
			services.AddRouting(x =>
			{
				x.LowercaseUrls = true;
			});
			services.AddAutoMapper(typeof(AutoMapperProfile));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			ServiceProvider.Instance = app.ApplicationServices;

			PrintEnvironment(app.ApplicationServices);

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

			app.UseRouting();

			// app.UseAuthentication();
			// app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHealthChecks("/health");
				endpoints.MapControllerRoute(
					"default",
					"{controller=Home}/{action=Index}/{id?}");
			});

			SeedData.InitializeAsync(new PortalOptions(Configuration), app.ApplicationServices).GetAwaiter()
				.GetResult();

			app.UseQuartz(true);
		}


		private void PrintEnvironment(IServiceProvider serviceProvider)
		{
			var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();
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
