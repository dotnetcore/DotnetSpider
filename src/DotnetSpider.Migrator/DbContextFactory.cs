using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;

namespace DotnetSpider.Migrator
{
	public class DbContextFactory : IDesignTimeDbContextFactory<DotnetSpiderDbContext>
	{
		private readonly string _connectionString;

		public DbContextFactory()
		{
			_connectionString = GetConnectionString();
		}

		public DbContextFactory(string connectionString)
		{
			_connectionString = connectionString;
		}

		public DotnetSpiderDbContext CreateDbContext(string[] args)
		{
			var builder = new DbContextOptionsBuilder<DotnetSpiderDbContext>();
			builder.UseSqlServer(_connectionString);
			return new DotnetSpiderDbContext(builder.Options);
		}

		private string GetConnectionString()
		{
			var builder = new ConfigurationBuilder();
			builder.AddJsonFile("appsettings.json", optional: false);

			var configuration = builder.Build();

			return configuration.GetConnectionString("DefaultConnection");
		}
	}
}
