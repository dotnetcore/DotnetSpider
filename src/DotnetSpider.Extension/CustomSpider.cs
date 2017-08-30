using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Monitor;
using NLog;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using System.Linq;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Extension
{
	[Obsolete]
	public abstract class CustomSpider : IRunable, INamed, IIdentity, ITask
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		private bool _exited;

		private Task _statusReporter;

		public string Name { get; set; }

		public string Identity { get; set; }

		public string TaskId { get; set; }

		public DbMonitor Monitor { get; set; }

		protected CustomSpider(string name)
		{
			Name = name;
		}

		protected abstract void ImplementAction(params string[] arguments);

		protected event Action OnExited;

		public void Run(params string[] arguments)
		{
			if (string.IsNullOrEmpty(Identity) || Identity.Length > 120)
			{
				throw new ArgumentException("Length of Identity should between 1 and 120.");
			}

			Monitor = new DbMonitor(Identity);

			try
			{
				Logger.MyLog(Identity, $"Start: {Name}", LogLevel.Info);

				if (Core.Environment.SystemConnectionStringSettings != null)
				{
					InsertRunningState();

					_statusReporter = Task.Factory.StartNew(() =>
					{
						while (!_exited)
						{
							try
							{
								Monitor.Report("Running",
									-1,
									-1,
									-1,
									-1,
									0,
									0,
									0,
									-1);
							}
							catch (Exception e)
							{
								Logger.MyLog(Identity, $"Report status failed: {e}.", LogLevel.Error);
							}
							Thread.Sleep(5000);
						}
					});
				}

				if (arguments.Contains("rerun") || arguments.Contains("validate"))
				{
					Verification.RemoveVerifidationLock(Identity);
				}

				ImplementAction(arguments);

				_exited = true;
				_statusReporter.Wait();

				Logger.MyLog(Identity, $"Complete: {Name}", LogLevel.Info);

				if (Core.Environment.SystemConnectionStringSettings != null)
				{
					try
					{
						Monitor.Report("Finished",
							-1,
							-1,
							-1,
							-1,
							0,
							0,
							0,
							-1);
					}
					catch (Exception e)
					{
						Logger.MyLog(Identity, $"Report status failed: {e}.", LogLevel.Error);
					}
				}
				if (OnExited != null)
				{
					Verification.ProcessVerifidation(Identity, OnExited);
				}
			}
			catch (Exception e)
			{
				Logger.MyLog(Identity, $"Terminated: {Name}: {e}", LogLevel.Error, e);

				if (Core.Environment.SystemConnectionStringSettings != null)
				{
					try
					{
						Monitor.Report("Terminated",
							-1,
							-1,
							-1,
							-1,
							0,
							0,
							0,
							-1);
					}
					catch (Exception e1)
					{
						Logger.MyLog(Identity, $"Report status failed: {e1}.", LogLevel.Error);
					}
				}
			}
			finally
			{
				if (Core.Environment.SystemConnectionStringSettings != null && !string.IsNullOrEmpty(TaskId))
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
			using (IDbConnection conn = Core.Environment.SystemConnectionStringSettings.GetDbConnection())
			{
				conn.Execute("CREATE SCHEMA IF NOT EXISTS `dotnetspider` DEFAULT CHARACTER SET utf8mb4;");
				conn.Execute("CREATE TABLE IF NOT EXISTS `dotnetspider`.`task_running` (`id` bigint(20) NOT NULL AUTO_INCREMENT, `taskId` varchar(120) NOT NULL, `name` varchar(200) NULL, `identity` varchar(120), `cdate` timestamp NOT NULL, PRIMARY KEY (id), UNIQUE KEY `taskId_unique` (`taskId`)) AUTO_INCREMENT=1");
				conn.Execute($"INSERT IGNORE INTO `dotnetspider`.`task_running` (`taskId`,`name`,`identity`,`cdate`) values ('{TaskId}','{Name}','{Identity}','{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}');");
			}
		}

		private void RemoveRunningState()
		{
			using (IDbConnection conn = Core.Environment.SystemConnectionStringSettings.GetDbConnection())
			{
				conn.Execute($"DELETE FROM `dotnetspider`.`task_running` WHERE `identity`='{Identity}';");
			}
		}
	}
}
