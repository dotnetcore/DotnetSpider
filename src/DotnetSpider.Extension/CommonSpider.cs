using System;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using System.Threading;
using DotnetSpider.Extension.Infrastructure;
using System.Data;
using NLog;
using Dapper;
using DotnetSpider.Extension.Monitor;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Extension
{
	public abstract class CommonSpider : Spider, ITask
	{
		protected const string InitStatusSetKey = "dotnetspider:init-stats";

		protected abstract void MyInit(params string[] arguments);

		protected Action DataVerificationAndReport;

		public bool UseDbMonitor { get; set; } = true;

		public string InitLockKey => $"dotnetspider:initLocker:{Identity}";

		protected CommonSpider(Site site) : base(site)
		{
		}

		public CommonSpider(string name, Site site) : base(site)
		{
			Name = name;
		}
		public CommonSpider(string name) : base(new Site())
		{
			Name = name;
		}

		public override void Run(params string[] arguments)
		{
			PrintInfo.Print();

			Logger.MyLog(Identity, "Build custom component...", LogLevel.Info);

			MyInit(arguments);

			if (string.IsNullOrEmpty(Identity) || Identity.Length > 120)
			{
				throw new ArgumentException("Length of Identity should between 1 and 120.");
			}

			if (arguments.Contains("skip"))
			{
				EmptySleepTime = 1000;

				if (Pipelines == null || Pipelines.Count == 0)
				{
					AddPipeline(new NullPipeline());
				}
				if (PageProcessors == null || PageProcessors.Count == 0)
				{
					AddPageProcessor(new NullPageProcessor());
				}
			}

			try
			{
				RegisterControl(this);

				InsertRunningState();

				base.Run(arguments);

				if (IsComplete && DataVerificationAndReport != null)
				{
					BaseVerification.ProcessVerifidation(Identity, DataVerificationAndReport);
				}
			}
			finally
			{
				RemoveRunningState();
			}
		}

		public ISpider ToDefaultSpider()
		{
			return new DefaultSpider("", new Site());
		}

		protected override void PreInitComponent(params string[] arguments)
		{
			base.PreInitComponent();

			if (UseDbMonitor)
			{
				Monitor = new DbMonitor(Identity);
			}

			if (Site == null)
			{
				throw new SpiderException("Site should not be null.");
			}

			Scheduler.Init(this);

			if (arguments.Contains("rerun"))
			{
				Scheduler.Clear();
				Scheduler.Dispose();
				BaseVerification.RemoveVerifidationLock(Identity);
			}
		}

		protected override void AfterInitComponent(params string[] arguments)
		{
			RedisConnection.Default?.Database.LockRelease(InitLockKey, 0);
			base.AfterInitComponent(arguments);
		}

		/// <summary>
		/// 分布式任务时使用, 只需要调用一次
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		protected bool IfRequireInitStartRequests(string[] arguments)
		{
			if (RedisConnection.Default != null)
			{
				if (arguments.Contains("rerun"))
				{
					RedisConnection.Default.Database.HashDelete(InitStatusSetKey, Identity);
					RedisConnection.Default.Database.LockRelease(InitLockKey, "0");
					return true;
				}
				else
				{
					while (!RedisConnection.Default.Database.LockTake(InitLockKey, "0", TimeSpan.FromMinutes(10)))
					{
						Thread.Sleep(1000);
					}
					var lockerValue = RedisConnection.Default.Database.HashGet(InitStatusSetKey, Identity);
					return lockerValue != "init complete";
				}
			}
			else
			{
				return true;
			}
		}

		protected void InsertRunningState()
		{
			if (Env.SystemConnectionStringSettings != null && !string.IsNullOrEmpty(TaskId))
			{
				using (IDbConnection conn = Env.SystemConnectionStringSettings.GetDbConnection())
				{
					conn.Execute("CREATE SCHEMA IF NOT EXISTS `DotnetSpider` DEFAULT CHARACTER SET utf8mb4;");
					conn.Execute("CREATE TABLE IF NOT EXISTS `DotnetSpider`.`TaskRunning` (`__Id` bigint(20) NOT NULL AUTO_INCREMENT, `TaskId` varchar(120) NOT NULL, `Name` varchar(200) NULL, `Identity` varchar(120), `CDate` timestamp NOT NULL DEFAULT current_timestamp, PRIMARY KEY (__Id), UNIQUE KEY `taskId_unique` (`TaskId`)) AUTO_INCREMENT=1");
					conn.Execute($"INSERT IGNORE INTO `DotnetSpider`.`TaskRunning` (`TaskId`,`Name`,`Identity`) values ('{TaskId}','{Name}','{Identity}');");
				}
			}
		}

		protected void RemoveRunningState()
		{
			if (Env.SystemConnectionStringSettings != null && !string.IsNullOrEmpty(TaskId))
			{
				using (IDbConnection conn = Env.SystemConnectionStringSettings.GetDbConnection())
				{
					conn.Execute($"DELETE FROM `DotnetSpider`.`TaskRunning` WHERE `Identity`='{Identity}';");
				}
			}
		}

		protected void RegisterControl(ISpider spider)
		{
			if (RedisConnection.Default != null)
			{
				try
				{
					RedisConnection.Default.Subscriber.Subscribe($"{spider.Identity}", (c, m) =>
					{
						switch (m)
						{
							case "PAUSE":
								{
									spider.Pause();
									break;
								}
							case "CONTINUE":
								{
									spider.Contiune();
									break;
								}
							case "RUNASYNC":
								{
									spider.RunAsync();
									break;
								}
							case "EXIT":
								{
									spider.Exit();
									break;
								}
						}
					});
				}
				catch (Exception e)
				{
					Logger.MyLog(Identity, "Register contol failed.", LogLevel.Error, e);
				}
			}
		}
	}
}
