using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotnetSpider.Broker.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotnetSpider.Broker.Hubs;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DotnetSpider.Broker.Test")]

namespace DotnetSpider.Broker
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
			services.AddResponseCompression();

			services.Configure<CookiePolicyOptions>(ops =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				ops.CheckConsentNeeded = context => true;
				ops.MinimumSameSitePolicy = SameSiteMode.None;
			});

			services.AddDbContext<BrokerDbContext>(ops =>
				ops.UseSqlServer(
					Configuration.GetConnectionString("DefaultConnection")));
			services.AddDefaultIdentity<IdentityUser>()
				.AddEntityFrameworkStores<BrokerDbContext>();

			services.AddMvc(o => o.Filters.Add<HttpGlobalExceptionFilter>()).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			var options = Configuration.GetSection("Broker").Get<BrokerOptions>();
			options.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
			services.AddSingleton(options);


		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env == null || env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}
			app.UseResponseCompression();
			app.UseSignalR(routes =>
			{
				routes.MapHub<WorkerHub>("/worker");
			});
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();
			app.UseMiddleware<ApiAuthorizeMiddleware>();
			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
