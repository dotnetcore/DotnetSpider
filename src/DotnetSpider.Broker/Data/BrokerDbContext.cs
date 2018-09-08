using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotnetSpider.Broker.Data
{
	public class BrokerDbContext : IdentityDbContext
	{
		public virtual DbSet<Node> Node { get; set; }

		public virtual DbSet<Block> Block { get; set; }

		public virtual DbSet<JobProperty> JobProperty { get; set; }

		public virtual DbSet<JobStatus> JobStatus { get; set; }

		public virtual DbSet<NodeStatus> NodeStatus { get; set; }

		public virtual DbSet<Running> Running { get; set; }

		public virtual DbSet<Worker> Worker { get; set; }

		public BrokerDbContext(DbContextOptions<BrokerDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<Worker>().HasIndex("FullClassName", "ConnectionId").IsUnique();
			base.OnModelCreating(builder);
		}
	}
}
