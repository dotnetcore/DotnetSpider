using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper;
using DotnetSpider.Common;
using DotnetSpider.Kafka;
using DotnetSpider.Portal.Entity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Core;

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
			
			services.AddKafkaEventBus();
			services.AddDownloadCenter(x => x.UseMySqlDownloaderAgentStore());
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
			Common.ServiceProvider.Instance = app.ApplicationServices;

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
							    $"SELECT count(*)  FROM information_schema.TABLES WHERE table_name ='QRTZ_FIRED_TRIGGERS';") ==
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
							    $"SELECT COUNT(*) from sysobjects WHERE id = object_id(N'[dbo].[QRTZ_FIRED_TRIGGERS]') AND OBJECTPROPERTY(id, N'') = 1IsUserTable") ==
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

				InitializedData(context);
			}
		}

		private void InitializedData(PortalDbContext context)
		{
			if (!context.DockerRepositories.Any())
			{
				context.DockerRepositories.Add(new DockerRepository
				{
					Name = "default",
					Schema = "http",
					Registry = "registry.zousong.com:5000",
					Repository = "dotnetspider/spiders.startup",
					CreationTime = DateTime.Now,
					UserName = "",
					Password = ""
				});
				context.SaveChanges();
			}
		}

		private void PrintEnvironment(ILogger logger)
		{
			Framework.PrintInfo();
			foreach (var kv in Configuration.GetChildren())
			{
				logger.LogInformation($"运行参数   : {kv.Key} = {kv.Value}", 0, ConsoleColor.DarkYellow);
			}


			logger.LogInformation($"运行目录   : {AppDomain.CurrentDomain.BaseDirectory}", 0,
				ConsoleColor.DarkYellow);
			logger.LogInformation(
				$"操作系统   : {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "X64" : "X86")}", 0,
				ConsoleColor.DarkYellow);
		}
	}
}