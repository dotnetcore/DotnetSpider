using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Infrastructure;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Extension
{
	public abstract class CustomSpider : IRunable, INamed, IIdentity, ITask
	{
		private bool _exited;

		private Task _statusReporter;

		public string Name { get; set; }

		public string ConnectString { get; set; }

		public string Identity { get; set; }

		public string TaskId { get; set; }

		protected CustomSpider(string name)
		{
			Name = name;
			if (string.IsNullOrEmpty(ConnectString))
			{
				ConnectString = Core.Infrastructure.Config.ConnectString;
			}
		}

		protected abstract void ImplementAction(params string[] arguments);
		protected virtual void VerifyData() { }

		public void Run(params string[] arguments)
		{
			if (string.IsNullOrEmpty(Identity) || Identity.Length > 120)
			{
				throw new ArgumentException("Length of Identity should between 1 and 120.");
			}

			if (string.IsNullOrEmpty(ConnectString))
			{
				ConnectString = Core.Infrastructure.Config.ConnectString;
			}

			if (string.IsNullOrEmpty(ConnectString))
			{
				throw new ArgumentException("ConnectString is missing.");
			}

			if (!string.IsNullOrEmpty(ConnectString))
			{
				if (!string.IsNullOrEmpty(TaskId))
				{
					InsertRunningState();
				}

				using (IDbConnection conn = new MySqlConnection(ConnectString))
				{
					conn.Open();
					var command = conn.CreateCommand();
					command.CommandType = CommandType.Text;

					command.CommandText = $"insert ignore into dotnetspider.status (`identity`, `status`,`thread`, `left`, `success`, `error`, `total`, `avgdownloadspeed`, `avgprocessorspeed`, `avgpipelinespeed`, `logged`) values('{Identity}', 'Init',-1, -1, -1, -1, -1, -1, -1, -1, '{DateTime.Now}');";
					command.ExecuteNonQuery();

					InsertLog(conn, "Info", $"开始任务: {Name}");
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
						InsertLog(conn, "Info", $"结束任务: {Name}");

						var command = conn.CreateCommand();
						command.CommandType = CommandType.Text;
						command.CommandText = $"update dotnetspider.status set `status`='Finished',`logged`='{DateTime.Now}' WHERE identity='{Identity}';";
						command.ExecuteNonQuery();
					}
				}
				Verifier.ProcessVerifidation(Identity, VerifyData);
			}
			catch (Exception e)
			{
				if (!string.IsNullOrEmpty(ConnectString))
				{
					using (IDbConnection conn = new MySqlConnection(ConnectString))
					{
						conn.Open();

						InsertLog(conn, "Info", $"退出任务: {Name}", e.ToString());

						var command = conn.CreateCommand();
						command.CommandType = CommandType.Text;
						command.CommandText = $"update dotnetspider.status set `status`='Exited', `logged`='{DateTime.Now}' WHERE identity='{Identity}';";
						command.ExecuteNonQuery();
					}
				}
			}
			finally
			{
				if (!string.IsNullOrEmpty(ConnectString) && !string.IsNullOrEmpty(TaskId))
				{
					RemoveRunningState();
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

		protected void InsertLog(IDbConnection conn, string level, string message, string exception = null)
		{
			var command = conn.CreateCommand();
			command.CommandType = CommandType.Text;

			command.CommandText = $"insert into dotnetspider.log (identity, node, logged, level, message,exception) values (@identity, @node, @logged, @level, @message, @exception)";

			var identity = command.CreateParameter();
			identity.ParameterName = "@identity";
			identity.DbType = DbType.String;
			identity.Value = Identity;
			command.Parameters.Add(identity);

			var node = command.CreateParameter();
			node.ParameterName = "@node";
			node.DbType = DbType.String;
			node.Value = NodeId.Id;
			command.Parameters.Add(node);

			var logged = command.CreateParameter();
			logged.ParameterName = "@logged";
			logged.DbType = DbType.DateTime;
			logged.Value = DateTime.Now;
			command.Parameters.Add(logged);

			var level2 = command.CreateParameter();
			level2.ParameterName = "@level";
			level2.DbType = DbType.String;
			level2.Value = level;
			command.Parameters.Add(level2);

			var message2 = command.CreateParameter();
			message2.ParameterName = "@message";
			message2.DbType = DbType.String;
			message2.Value = message;
			command.Parameters.Add(message2);

			var exception2 = command.CreateParameter();
			exception2.ParameterName = "@exception";
			exception2.DbType = DbType.String;
			exception2.Value = exception;
			command.Parameters.Add(exception2);

			command.ExecuteNonQuery();
		}

		private void InsertRunningState()
		{
			using (IDbConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Open();
				var command = conn.CreateCommand();
				command.CommandType = CommandType.Text;

				command.CommandText = "CREATE SCHEMA IF NOT EXISTS `dotnetspider` DEFAULT CHARACTER SET utf8mb4;";
				command.ExecuteNonQuery();

				command.CommandText = "CREATE TABLE IF NOT EXISTS `dotnetspider`.`task_running` (`id` bigint(20) NOT NULL AUTO_INCREMENT, `taskId` varchar(120) NOT NULL, `name` varchar(200) NULL, `identity` varchar(120), `cdate` timestamp NOT NULL, PRIMARY KEY (id), UNIQUE KEY `taskId_unique` (`taskId`)) ENGINE=InnoDB AUTO_INCREMENT=1  DEFAULT CHARSET=utf8";
				command.ExecuteNonQuery();

				command.CommandText = $"INSERT IGNORE INTO `dotnetspider`.`task_running` (`taskId`,`name`,`identity`,`cdate`) values ('{TaskId}','{Name}','{Identity}','{DateTime.Now}');";
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

				command.CommandText = $"DELETE FROM `dotnetspider`.`task_running` WHERE `identity`='{Identity}';";
				command.ExecuteNonQuery();
			}
		}
	}
}
