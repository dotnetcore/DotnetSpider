using System;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using System.Data;
using MySql.Data.MySqlClient;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension
{
	public abstract class EntitySpiderBuilder : IControllable
	{
		protected string _userId;
		protected string _name;
		protected string _batch;

		protected abstract EntitySpider GetEntitySpider();
		protected EntitySpider Spider { get; private set; }

		protected EntitySpiderBuilder(string userId, string name, Batch batch)
		{
			SetInfo(userId, name, batch);
		}

		protected void SetInfo(string userId, string name, Batch batch)
		{
			_userId = userId;
			_name = name;
			if (string.IsNullOrEmpty(_userId))
			{
				_userId = "DotnetSpider";
			}

			if (string.IsNullOrEmpty(_name) || _name.Length > 120)
			{
				throw new ArgumentException("Length of name should between 1 and 120.");
			}

			switch (batch)
			{
				case Batch.Now:
					{
						_batch = DateTime.Now.ToString("yyyy_MM_dd_hhmmss");
						break;
					}
				case Batch.Daily:
					{
						_batch = DateTimeUtils.RunIdOfToday;
						break;
					}
				case Batch.Weekly:
					{
						_batch = DateTimeUtils.RunIdOfMonday;
						break;
					}
				case Batch.Monthly:
					{
						_batch = DateTimeUtils.RunIdOfMonthly;
						break;
					}
			}
		}

		public string ConnectString { get; set; }

		public virtual void Run(params string[] args)
		{
			if (string.IsNullOrEmpty(ConnectString))
			{
				ConnectString = Core.Infrastructure.Configuration.GetValue(Core.Infrastructure.Configuration.LogAndStatusConnectString);
			}
			Spider = GetEntitySpider();
			if (Spider == null)
			{
				throw new SpiderException("Spider is null.");
			}
			PrepareDb();
			InsertTask();
			InsertBatch();
			Spider.Run(args);
		}

		private void PrepareDb()
		{
			if (!string.IsNullOrEmpty(ConnectString))
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
		}

		private void InsertTask()
		{
			if (!string.IsNullOrEmpty(ConnectString))
			{
				using (IDbConnection conn = new MySqlConnection(ConnectString))
				{
					conn.Open();
					var command = conn.CreateCommand();
					command.CommandType = CommandType.Text;

					command.CommandText = "CREATE TABLE IF NOT EXISTS `dotnetspider`.`tasks` (`id` bigint(20) NOT NULL AUTO_INCREMENT, `name` varchar(120) NOT NULL, `userId` varchar(120) NOT NULL, `cdate` timestamp NOT NULL, PRIMARY KEY (id), UNIQUE KEY `userId_name_unique` (`userId`,`name`)) ENGINE=InnoDB AUTO_INCREMENT=1  DEFAULT CHARSET=utf8";
					command.ExecuteNonQuery();

					command.CommandText = $"INSERT IGNORE INTO `dotnetspider`.`tasks` (`name`,`userId`,`cdate`) values ('{_name}','{_userId}','{DateTime.Now}');";
					command.ExecuteNonQuery();
				}
			}
		}

		private void InsertBatch()
		{
			if (!string.IsNullOrEmpty(ConnectString))
			{
				using (IDbConnection conn = new MySqlConnection(ConnectString))
				{
					conn.Open();
					var command = conn.CreateCommand();
					command.CommandType = CommandType.Text;

					command.CommandText = "CREATE TABLE IF NOT EXISTS `dotnetspider`.`task_batches` (`id` bigint AUTO_INCREMENT, `taskId` bigint(20) NOT NULL, `batch` timestamp NOT NULL, `code` varchar(32) NOT NULL, PRIMARY KEY (`id`), INDEX `taskId_index` (`taskId`)) ENGINE=InnoDB AUTO_INCREMENT=1  DEFAULT CHARSET=utf8";
					command.ExecuteNonQuery();

					command.CommandText = $"SELECT id FROM `dotnetspider`.`tasks` WHERE `userId` = '{_userId}' and `name` = '{_name}';";
					var result = command.ExecuteScalar();
					if (result != null)
					{
						var taskId = Convert.ToInt32(result);
						var identity = Encrypt.Md5Encrypt($"{_name}{_batch}");
						command.CommandText = $"INSERT IGNORE INTO `dotnetspider`.`task_batches` (`taskId`,`batch`, `code`) values ('{taskId}','{DateTime.Now}','{identity}');";
						command.ExecuteNonQuery();

						Spider.Identity = identity;
					}
					else
					{
						throw new ArgumentException("Task info is missing.");
					}
				}
			}
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
	}
}