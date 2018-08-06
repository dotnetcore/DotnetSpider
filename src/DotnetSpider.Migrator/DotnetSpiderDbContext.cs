using DotnetSpider.Common;
using DotnetSpider.Common.Entity;
using Microsoft.EntityFrameworkCore;

namespace DotnetSpider.Migrator
{
	/// <summary>
	/// 此类只是用来做建库、建表、迁移工作。并不会在实际业务中使用 EF
	/// </summary>
	public class DotnetSpiderDbContext : DbContext
	{
		public DbSet<Block> Block { get; set; }

		public DbSet<Node> Node { get; set; }

		public DbSet<NodeHeartbeat> NodeHeartbeat { get; set; }

		public DbSet<RequestQueue> RequestQueue { get; set; }

		public DbSet<RunningHistory> RunningHistory { get; set; }

		public DbSet<Running> Running { get; set; }

		public DotnetSpiderDbContext(DbContextOptions options)
			: base(options)
		{

		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<RequestQueue>().HasKey(t => new { t.RequestId, t.Identity });
			modelBuilder.Entity<Block>().HasKey(t => new { t.BlockId, t.Identity });
			base.OnModelCreating(modelBuilder);
		}
	}
}
