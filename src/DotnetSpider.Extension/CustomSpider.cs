using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Monitor;
using NLog;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Extension
{
	[Obsolete]
	public abstract class CustomSpider : IAppBase
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

            Monitor = new DbMonitor(TaskId, Identity);

            try
            {
				Logger.AllLog(Identity, $"Start: {Name}", LogLevel.Info);

				if (Env.SystemConnectionStringSettings != null)
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
								Logger.AllLog(Identity, $"Report status failed: {e}.", LogLevel.Error);
							}
							Thread.Sleep(5000);
						}
					});
				}

				if (arguments.Contains("rerun") || arguments.Contains("validate"))
				{
					BaseVerification.RemoveVerifidationLock(Identity);
				}

				ImplementAction(arguments);

				_exited = true;
				_statusReporter.Wait();

				Logger.AllLog(Identity, $"Complete: {Name}", LogLevel.Info);

				if (Env.SystemConnectionStringSettings != null)
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
						Logger.AllLog(Identity, $"Report status failed: {e}.", LogLevel.Error);
					}
				}
				if (OnExited != null)
				{
					BaseVerification.ProcessVerifidation(Identity, OnExited);
				}
			}
			catch (Exception e)
			{
				Logger.AllLog(Identity, $"Terminated: {Name}: {e}", LogLevel.Error, e);

				if (Env.SystemConnectionStringSettings != null)
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
						Logger.AllLog(Identity, $"Report status failed: {e1}.", LogLevel.Error);
					}
				}
			}
			finally
			{
				if (Env.SystemConnectionStringSettings != null && !string.IsNullOrEmpty(TaskId))
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
			using (IDbConnection conn = Env.SystemConnectionStringSettings.GetDbConnection())
			{
				conn.MyExecute("CREATE SCHEMA IF NOT EXISTS `DotnetSpider` DEFAULT CHARACTER SET utf8mb4;");
				conn.MyExecute("CREATE TABLE IF NOT EXISTS `DotnetSpider`.`TaskRunning` (`__Id` bigint(20) NOT NULL AUTO_INCREMENT, `TaskId` varchar(120) NOT NULL, `Name` varchar(200) NULL, `Identity` varchar(120), `CDate` timestamp NOT NULL, PRIMARY KEY (__Id), UNIQUE KEY `taskId_unique` (`TaskId`)) AUTO_INCREMENT=1");
				conn.MyExecute($"INSERT IGNORE INTO `DotnetSpider`.`TaskRunning` (`TaskId`,`Name`,`Identity`,`CDate`) values ('{TaskId}','{Name}','{Identity}','{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}');");
			}
		}

		private void RemoveRunningState()
		{
			using (IDbConnection conn = Env.SystemConnectionStringSettings.GetDbConnection())
			{
				conn.MyExecute($"DELETE FROM `DotnetSpider`.`TaskRunning` WHERE `Identity`='{Identity}';");
			}
		}
	}
}
