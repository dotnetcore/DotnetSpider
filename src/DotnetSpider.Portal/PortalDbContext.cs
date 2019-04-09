using DotnetSpider.Core;
using DotnetSpider.Portal.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Portal
{
	public class PortalDbContext : DbContext, IDesignTimeDbContextFactory<PortalDbContext>
	{
		public DbSet<DockerImageRepository> DockerImageRepositories { get; set; }

		public DbSet<DockerImage> DockerImages { get; set; }

		public PortalDbContext()
		{
		}

		public PortalDbContext(DbContextOptions<PortalDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<DockerImageRepository>().HasIndex(x => x.Name).IsUnique();
			builder.Entity<DockerImageRepository>().HasIndex(x => x.Repository).IsUnique();
			builder.Entity<DockerImageRepository>().HasIndex(x => x.CreationTime);

			builder.Entity<DockerImage>().HasIndex(x => x.Repository).IsUnique();
		}

		public PortalDbContext CreateDbContext(string[] args)
		{
			var builder = new DbContextOptionsBuilder<PortalDbContext>();

			var configuration = Framework.CreateConfiguration(args.Length > 0 ? args[0] : "appsettings.json");
			var options = new PortalOptions(configuration);
			switch (options.Database?.ToLower())
			{
				case "mysql":
				{
					builder.UseMySql(options.ConnectionString);
					break;
				}
				default:
				{
					builder.UseSqlServer(options.ConnectionString);
					break;
				}
			}

			return new PortalDbContext(builder.Options);
		}
	}
}