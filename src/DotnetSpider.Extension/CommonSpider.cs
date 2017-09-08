using System;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using System.Threading;
using DotnetSpider.Extension.Infrastructure;
using System.Data;
using System.Reflection;
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

		public string TaskId { get; set; }

		public bool UseDbLog { get; set; } = true;

		public string InitLockKey => $"dotnetspider:initLocker:{Identity}";

		protected CommonSpider(string name) : this(name, new Site())
		{
		}

		protected CommonSpider(string name, Site site) : base(site)
		{
			Name = name;
		}

		public override void Run(params string[] arguments)
		{
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
				if (ReadOnlyPageProcessors == null || ReadOnlyPageProcessors.Count == 0)
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

			if (UseDbLog)
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
			if (Core.Environment.SystemConnectionStringSettings != null && !string.IsNullOrEmpty(TaskId))
			{
				using (IDbConnection conn = Core.Environment.SystemConnectionStringSettings.GetDbConnection())
				{
					conn.Execute("CREATE SCHEMA IF NOT EXISTS `dotnetspider` DEFAULT CHARACTER SET utf8mb4;");
					conn.Execute("CREATE TABLE IF NOT EXISTS `dotnetspider`.`task_running` (`id` bigint(20) NOT NULL AUTO_INCREMENT, `taskId` varchar(120) NOT NULL, `name` varchar(200) NULL, `identity` varchar(120), `cdate` timestamp NOT NULL, PRIMARY KEY (id), UNIQUE KEY `taskId_unique` (`taskId`)) AUTO_INCREMENT=1");
					conn.Execute($"INSERT IGNORE INTO `dotnetspider`.`task_running` (`taskId`,`name`,`identity`,`cdate`) values ('{TaskId}','{Name}','{Identity}','{DateTime.Now}');");
				}
			}
		}

		protected void RemoveRunningState()
		{
			if (Core.Environment.SystemConnectionStringSettings != null && !string.IsNullOrEmpty(TaskId))
			{
				using (IDbConnection conn = Core.Environment.SystemConnectionStringSettings.GetDbConnection())
				{
					conn.Execute($"DELETE FROM `dotnetspider`.`task_running` WHERE `identity`='{Identity}';");
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
