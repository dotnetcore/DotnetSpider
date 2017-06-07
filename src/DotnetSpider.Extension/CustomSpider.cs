using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Infrastructure;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Extension
{
	public abstract class CustomSpider : IRunable, INamed
	{
		private string _name;
		private string _batchStr;
		private bool _exited;

		private Task _statusReporter;

		public string Name => _name;

		public string ConnectString { get; set; }

		public string Identity { get; private set; }

		protected CustomSpider(string userId, string name, Batch batch)
		{
			_name = name;

			if (string.IsNullOrEmpty(_name) || _name.Length > 120)
			{
				throw new ArgumentException("Length of name should between 1 and 120.");
			}

			switch (batch)
			{
				case Batch.Now:
					{
						_batchStr = DateTime.Now.ToString("yyyy_MM_dd_hhmmss");
						break;
					}
				case Batch.Daily:
					{
						_batchStr = DateTimeUtils.RunIdOfToday;
						break;
					}
				case Batch.Weekly:
					{
						_batchStr = DateTimeUtils.RunIdOfMonday;
						break;
					}
				case Batch.Monthly:
					{
						_batchStr = DateTimeUtils.RunIdOfMonthly;
						break;
					}
			}
		}

		protected abstract void ImplementAction(params string[] arguments);

		public void Run(params string[] arguments)
		{
			if (string.IsNullOrEmpty(ConnectString))
			{
				ConnectString = Core.Infrastructure.Configuration.GetValue(Core.Infrastructure.Configuration.LogAndStatusConnectString);
			}

			if (!string.IsNullOrEmpty(ConnectString))
			{
				PrepareDb();
				InsertTask();
				InsertBatch();

				using (IDbConnection conn = new MySqlConnection(ConnectString))
				{
					conn.Open();
					var command = conn.CreateCommand();
					command.CommandType = CommandType.Text;

					command.CommandText = $"insert ignore into dotnetspider.status (`identity`, `status`,`thread`, `left`, `success`, `error`, `total`, `avgdownloadspeed`, `avgprocessorspeed`, `avgpipelinespeed`, `logged`) values('{Identity}', 'Init',-1, -1, -1, -1, -1, -1, -1, -1, '{DateTime.Now}');";
					command.ExecuteNonQuery();

					var message = $"开始任务: {_name}";
					command.CommandText = $"insert into dotnetspider.log (identity, node, logged, level, message) values ('{Identity}', '{NodeId.Id}', '{DateTime.Now}', 'Info', '{message}');";
					command.ExecuteNonQuery();
				}

				_statusReporter = Task.Factory.StartNew(() =>
				{
					using (IDbConnection conn = new MySqlConnection(ConnectString))
					{
						conn.Open();
						var command = conn.CreateCommand();
						command.CommandType = CommandType.Text;

						while (!_exited)
						{
							command.CommandText = $"update dotnetspider.status set `logged`='{DateTime.Now}' WHERE identity='{Identity}';";
							command.ExecuteNonQuery();
							Thread.Sleep(5000);
						}
					}
				});
			}

			try
			{
				ImplementAction(arguments);

				_exited = true;
				_statusReporter.Wait();

				if (!string.IsNullOrEmpty(ConnectString))
				{
					using (IDbConnection conn = new MySqlConnection(ConnectString))
					{
						conn.Open();
						var command = conn.CreateCommand();
						command.CommandType = CommandType.Text;

						var message = $"结束任务: {_name}";
						command.CommandText = $"insert into dotnetspider.log (identity, node, logged, level, message) values ('{Identity}','{NodeId.Id}', '{DateTime.Now}', 'Info', '{message}');";
						command.ExecuteNonQuery();

						command.CommandText = $"update dotnetspider.status set `status`='Finished',`logged`='{DateTime.Now}' WHERE identity='{Identity}';";
						command.ExecuteNonQuery();
					}
				}
			}
			catch (Exception e)
			{
				if (!string.IsNullOrEmpty(ConnectString))
				{
					using (IDbConnection conn = new MySqlConnection(ConnectString))
					{
						conn.Open();
						var command = conn.CreateCommand();
						command.CommandType = CommandType.Text;

						var message = $"退出任务: {_name}";
						command.CommandText = $"insert into dotnetspider.log (identity, node, logged, level, message, callsite, exception) values ('{Identity}','{NodeId.Id}','{DateTime.Now}', 'Info', '{message}','{e}','{e.Message}');";
						command.ExecuteNonQuery();

						command.CommandText = $"update dotnetspider.status set `status`='Exited' `logged`='{DateTime.Now}' WHERE identity='{Identity}';";
						command.ExecuteNonQuery();
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

		protected void PrepareDb()
		{
			using (IDbConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Open();
				var command = conn.CreateCommand();
				command.CommandType = CommandType.Text;

				command.CommandText = "CREATE SCHEMA IF NOT EXISTS `dotnetspider` DEFAULT CHARACTER SET utf8mb4;";
				command.ExecuteNonQuery();

				command.CommandText = "CREATE TABLE IF NOT EXISTS `dotnetspider`.`status` (`identity` varchar(120) NOT NULL,`logged` timestamp NULL DEFAULT NULL,`status` varchar(20) DEFAULT NULL,`thread` int(13),`left` bigint(20),`success` bigint(20),`error` bigint(20),`total` bigint(20),`avgdownloadspeed` float,`avgprocessorspeed` bigint(20),`avgpipelinespeed` bigint(20),PRIMARY KEY(`identity`)) ENGINE = InnoDB DEFAULT CHARSET = utf8;";
				command.ExecuteNonQuery();

				command.CommandText = "CREATE TABLE IF NOT EXISTS `dotnetspider`.`log` (`identity` varchar(120) NOT NULL, `node` varchar(120) NULL, `logged` timestamp NULL DEFAULT NULL,`level` varchar(20) DEFAULT NULL, `message` text, `callSite` text, `exception` text, `id` bigint(20) NOT NULL AUTO_INCREMENT, PRIMARY KEY (`id`), KEY `index01` (`identity`)) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;";
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

				command.CommandText = "CREATE TABLE IF NOT EXISTS `dotnetspider`.`tasks` (`id` bigint(20) NOT NULL AUTO_INCREMENT, `name` varchar(120) NOT NULL, `cdate` timestamp NOT NULL, PRIMARY KEY (id), UNIQUE KEY `name_unique` (`name`)) ENGINE=InnoDB AUTO_INCREMENT=1  DEFAULT CHARSET=utf8";
				command.ExecuteNonQuery();

				command.CommandText = $"INSERT IGNORE INTO `dotnetspider`.`tasks` (`name`,`cdate`) values ('{_name}','{DateTime.Now}');";
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

				command.CommandText = $"SELECT id FROM `dotnetspider`.`tasks` WHERE `name` = '{_name}';";
				var result = command.ExecuteScalar();
				if (result != null)
				{
					var taskId = Convert.ToInt32(result);
					var identity = Encrypt.Md5Encrypt($"{_name}{_batchStr}");
					command.CommandText = $"INSERT IGNORE INTO `dotnetspider`.`task_batches` (`taskId`,`batch`, `code`) values ('{taskId}','{DateTime.Now}','{identity}');";
					command.ExecuteNonQuery();

					Identity = identity;
				}
				else
				{
					throw new ArgumentException("Task info is missing.");
				}
			}
		}
	}
}
