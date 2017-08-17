using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Monitor;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using System.Linq;

namespace DotnetSpider.Extension
{
	public abstract class CustomSpider : IRunable, INamed, IIdentity, ITask
	{
		protected readonly static ILogger Logger = LogCenter.GetLogger();

		private bool _exited;

		private Task _statusReporter;

		public string Name { get; set; }

		public string ConnectString { get; set; }

		public string Identity { get; set; }

		public string TaskId { get; set; }

		protected CustomSpider(string name)
		{
			Name = name;
			ConnectString = Config.ConnectString;
		}

		protected abstract void ImplementAction(params string[] arguments);

		protected event Action OnExited;

		public void Run(params string[] arguments)
		{
			if (string.IsNullOrEmpty(Identity) || Identity.Length > 120)
			{
				throw new ArgumentException("Length of Identity should between 1 and 120.");
			}

			if (string.IsNullOrEmpty(ConnectString))
			{
				throw new ArgumentException("ConnectString is empty.");
			}

			if (!string.IsNullOrEmpty(ConnectString))
			{
				NLogUtil.PrepareDatabase(ConnectString);
				DbMonitor.InitStatusDatabase(ConnectString);

				InsertRunningState();

				using (IDbConnection conn = new MySqlConnection(ConnectString))
				{
					conn.Execute($"insert ignore into dotnetspider.status (`identity`, `status`,`thread`, `left`, `success`, `error`, `total`, `avgdownloadspeed`, `avgprocessorspeed`, `avgpipelinespeed`, `logged`) values('{Identity}', 'Init',-1, -1, -1, -1, -1, -1, -1, -1, '{DateTime.Now}');");
				}

				_statusReporter = Task.Factory.StartNew(() =>
				{
					using (IDbConnection conn = new MySqlConnection(ConnectString))
					{
						while (!_exited)
						{
							conn.Execute($"update dotnetspider.status set `logged`='{DateTime.Now}' WHERE identity='{Identity}';");
							Thread.Sleep(5000);
						}
					}
				});
			}

			try
			{
				Logger.MyLog(Identity, $"任务开始: {Name}", LogLevel.Info);

				if (arguments.Contains("rerun") || arguments.Contains("validate"))
				{
					Verifier.RemoveVerifidationLock(Identity);
				}

				ImplementAction(arguments);

				_exited = true;
				_statusReporter.Wait();

				Logger.MyLog(Identity, $"任务结束: {Name}", LogLevel.Info);

				if (!string.IsNullOrEmpty(ConnectString))
				{
					using (IDbConnection conn = new MySqlConnection(ConnectString))
					{
						conn.Execute($"update dotnetspider.status set `status`='Finished',`logged`='{DateTime.Now}' WHERE identity='{Identity}';");
					}
				}
				if (OnExited != null)
				{
					Verifier.ProcessVerifidation(Identity, OnExited);
				}
			}
			catch (Exception e)
			{
				Logger.MyLog(Identity, $"任务中止: {Name}: {e}", LogLevel.Error, e);

				if (!string.IsNullOrEmpty(ConnectString))
				{
					using (IDbConnection conn = new MySqlConnection(ConnectString))
					{
						conn.Execute($"update dotnetspider.status set `status`='Exited', `logged`='{DateTime.Now}' WHERE identity='{Identity}';");
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

		private void InsertRunningState()
		{
			using (IDbConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Execute("CREATE SCHEMA IF NOT EXISTS `dotnetspider` DEFAULT CHARACTER SET utf8mb4;");
				conn.Execute("CREATE TABLE IF NOT EXISTS `dotnetspider`.`task_running` (`id` bigint(20) NOT NULL AUTO_INCREMENT, `taskId` varchar(120) NOT NULL, `name` varchar(200) NULL, `identity` varchar(120), `cdate` timestamp NOT NULL, PRIMARY KEY (id), UNIQUE KEY `taskId_unique` (`taskId`)) ENGINE=InnoDB AUTO_INCREMENT=1  DEFAULT CHARSET=utf8");
				conn.Execute($"INSERT IGNORE INTO `dotnetspider`.`task_running` (`taskId`,`name`,`identity`,`cdate`) values ('{TaskId}','{Name}','{Identity}','{DateTime.Now}');");
			}
		}

		private void RemoveRunningState()
		{
			using (IDbConnection conn = new MySqlConnection(ConnectString))
			{
				conn.Execute($"DELETE FROM `dotnetspider`.`task_running` WHERE `identity`='{Identity}';");
			}
		}
	}
}
