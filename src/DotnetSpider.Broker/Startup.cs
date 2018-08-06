using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Broker.Services;
using DotnetSpider.Broker.Services.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetSpider.Broker
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddResponseCompression();

			services.AddMvc(o => o.Filters.Add<HttpGlobalExceptionFilter>()).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			var options = Configuration.GetSection("Broker").Get<BrokerOptions>();
			services.AddSingleton(options);

			switch (options.StorageType)
			{
				case StorageType.MySql:
					{
						services.AddScoped<INodeService, Services.MySql.NodeService>();
						services.AddSingleton<IRunningService, Services.MySql.RunningService>();
						services.AddSingleton<IBlockService, Services.MySql.BlockService>();
						services.AddSingleton<IRunningHistoryService, Services.MySql.RunningHistoryService>();
						services.AddSingleton<IRequestQueueService, Services.MySql.RequestQueueService>();
						break;
					}
				case StorageType.SqlServer:
					{
						services.AddScoped<INodeService, Services.NodeService>();
						services.AddSingleton<IRunningService, Services.RunningService>();
						services.AddSingleton<IBlockService, Services.BlockService>();
						services.AddSingleton<IRunningHistoryService, Services.RunningHistoryService>();
						services.AddSingleton<IRequestQueueService, Services.RequestQueueService>();
						break;
					}
			}
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseResponseCompression();
			app.UseStaticFiles();
			app.UseMiddleware<AuthorizeMiddleware>();
			// app.ApplicationServices.GetRequiredService<IInitializer>().Init();
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
