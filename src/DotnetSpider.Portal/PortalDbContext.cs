using DotnetSpider.Core;
using DotnetSpider.DownloadAgentRegisterCenter.Entity;
using DotnetSpider.Portal.Entity;
using DotnetSpider.Statistics.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotnetSpider.Portal
{
	public class PortalDbContext : DbContext, IDesignTimeDbContextFactory<PortalDbContext>
	{
		private readonly bool _isDesignTime;

		public DbSet<DockerRepository> DockerRepositories { get; set; }

		public DbSet<DockerImage> DockerImages { get; set; }

		public DbSet<Entity.Spider> Spiders { get; set; }

		public DbSet<Entity.SpiderContainer> SpiderContainers { get; set; }

		public PortalDbContext()
		{
			_isDesignTime = true;
		}

		public PortalDbContext(DbContextOptions<PortalDbContext> options, bool isDesignTime = false)
			: base(options)
		{
			_isDesignTime = isDesignTime;
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			if (!_isDesignTime)
			{
				builder.Model.AddEntityType(typeof(DownloaderAgent));
				builder.Model.AddEntityType(typeof(DownloaderAgentHeartbeat));
				builder.Model.AddEntityType(typeof(SpiderStatistics));
				builder.Model.AddEntityType(typeof(DownloadStatistics));
			}

			builder.Entity<DockerRepository>().HasIndex(x => x.Name).IsUnique();
			builder.Entity<DockerRepository>().HasIndex(x => x.Repository).IsUnique();
			builder.Entity<DockerRepository>().HasIndex(x => x.CreationTime);

			builder.Entity<DockerImage>().HasIndex(x => x.Image).IsUnique();

			builder.Entity<Entity.Spider>().HasIndex(x => x.Name);
			builder.Entity<Entity.Spider>().HasIndex(x => x.CreationTime);
			builder.Entity<Entity.Spider>().HasIndex(x => x.Image);

			builder.Entity<SpiderContainer>().HasIndex(x => x.ContainerId);
			builder.Entity<SpiderContainer>().HasIndex(x => x.CreationTime);
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

			return new PortalDbContext(builder.Options, true);
		}
	}
}