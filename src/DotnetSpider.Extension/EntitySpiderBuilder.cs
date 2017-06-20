using System;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using System.Data;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Extension
{
	public abstract class EntitySpiderBuilder : IControllable, INamed, IBatch
	{
		protected abstract EntitySpider GetEntitySpider();
		protected EntitySpider Spider { get; private set; }

		public string Name { get; set; }
		public string Batch { get; set; }

		public EntitySpiderBuilder(string name) : this(name, Infrastructure.Batch.Now)
		{
		}

		public EntitySpiderBuilder(string name, string batch)
		{
			SetInfo(name, batch);
		}

		protected void SetInfo(string name, string batch)
		{
			Name = name;

			if (string.IsNullOrEmpty(Name) || Name.Length > 120)
			{
				throw new ArgumentException("Length of name should between 1 and 120.");
			}

			Batch = batch;
		}

		public string ConnectString { get; set; }

		public virtual void Run(params string[] args)
		{
			if (string.IsNullOrEmpty(Name) || Name.Length > 120)
			{
				throw new ArgumentException("Length of name should between 1 and 120.");
			}

			if (string.IsNullOrEmpty(ConnectString))
			{
				ConnectString = Core.Infrastructure.Configuration.GetValue(Core.Infrastructure.Configuration.LogAndStatusConnectString);
			}
			Spider = GetEntitySpider();
			if (Spider == null)
			{
				throw new SpiderException("Spider is null.");
			}
			if (!string.IsNullOrEmpty(ConnectString))
			{
				PrepareDb();
				InsertTask();
				InsertBatch();
			}
			Spider.Run(args);

			RemoveRunningState();
		}

		public Task RunAsync(params string[] arguments)
		{
			return Task.Factory.StartNew(() =>
			{
				Run(arguments);
			});
		}

		public void Pause(Action action = null)
		{
			Spider.Pause(action);
		}

		public void Exit(Action action = null)
		{
			Spider.Exit(action);
		}

		public void Contiune()
		{
			Spider.Contiune();
		}

		private void PrepareDb()
		{
			using (IDbConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Open();
				var command = conn.CreateCommand();
				command.CommandType = CommandType.Text;

				command.CommandText = "CREATE SCHEMA IF NOT EXISTS `dotnetspider` DEFAULT CHARACTER SET utf8mb4;";
				command.ExecuteNonQuery();
			}
		}

		private void InsertTask()
		{
			using (IDbConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Open();
				var command = conn.CreateCommand();
				command.CommandType = CommandType.Text;

				command.CommandText = "CREATE TABLE IF NOT EXISTS `dotnetspider`.`task_running` (`id` bigint(20) NOT NULL AUTO_INCREMENT, `taskId` bigint(20) NOT NULL, `cdate` timestamp NOT NULL, PRIMARY KEY (id), UNIQUE KEY `taskId_unique` (`taskId`)) ENGINE=InnoDB AUTO_INCREMENT=1  DEFAULT CHARSET=utf8";
				command.ExecuteNonQuery();

				command.CommandText = $"INSERT IGNORE INTO `dotnetspider`.`task_running` (`taskId`,`cdate`) values ('{Name}','{DateTime.Now}');";
				command.ExecuteNonQuery();
			}
		}

		private void RemoveRunningState()
		{
			using (IDbConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Open();
				var command = conn.CreateCommand();
				command.CommandType = CommandType.Text;

				command.CommandText = $"DELETE FROM `dotnetspider`.`task_running` WHERE `taskId`='{Name}';";
				command.ExecuteNonQuery();
			}
		}

		private void InsertBatch()
		{
			using (IDbConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Open();
				var command = conn.CreateCommand();
				command.CommandType = CommandType.Text;

				command.CommandText = "CREATE TABLE IF NOT EXISTS `dotnetspider`.`task_batches` (`id` bigint AUTO_INCREMENT, `taskId` bigint(20) NOT NULL, `batch` timestamp NOT NULL, `code` varchar(32) NOT NULL, PRIMARY KEY (`id`), INDEX `taskId_index` (`taskId`)) ENGINE=InnoDB AUTO_INCREMENT=1  DEFAULT CHARSET=utf8";
				command.ExecuteNonQuery();

				var identity = Encrypt.Md5Encrypt($"{Name}{Batch}");
				command.CommandText = $"INSERT IGNORE INTO `dotnetspider`.`task_batches` (`taskId`,`batch`, `code`) values ('{Name}','{DateTime.Now}','{identity}');";
				command.ExecuteNonQuery();

				Spider.Identity = identity;
			}
		}
	}
}