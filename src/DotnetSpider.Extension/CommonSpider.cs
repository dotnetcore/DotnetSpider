using System;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using System.Threading;
using DotnetSpider.Extension.Infrastructure;
using System.Data;
using NLog;
using DotnetSpider.Extension.Monitor;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Redial;
using DotnetSpider.Core.Monitor;

namespace DotnetSpider.Extension
{
	public abstract class CommonSpider : Spider
	{
		private const string InitFinishedValue = "init complete";
		protected const string InitStatusSetKey = "dotnetspider:init-stats";

		protected abstract void MyInit(params string[] arguments);

		protected Action DataVerificationAndReport;

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

			Logger.AllLog(Identity, "Init redial module if necessary.", LogLevel.Info);

			InitRedialConfiguration();

			Logger.AllLog(Identity, "Build custom component...", LogLevel.Info);

			NetworkCenter.Current.Execute("myInit", () =>
			{
				MyInit(arguments);
			});

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

			RegisterControl(this);

			base.Run(arguments);

			if (IsComplete && DataVerificationAndReport != null)
			{
				NetworkCenter.Current.Execute("verifyAndReport", () =>
				{
					BaseVerification.ProcessVerifidation(Identity, DataVerificationAndReport);
				});
			}
		}

		protected virtual void InitRedialConfiguration() { }

		public ISpider ToDefaultSpider()
		{
			return new DefaultSpider("", new Site());
		}

		protected override void PreInitComponent(params string[] arguments)
		{
			base.PreInitComponent();

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
		protected override bool IfRequireInitStartRequests(string[] arguments)
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
					return lockerValue != InitFinishedValue;
				}
			}
			else
			{
				return true;
			}
		}

		protected override void InitStartRequestsFinished()
		{
			if (RedisConnection.Default != null)
			{
				RedisConnection.Default.Database.HashSet(InitStatusSetKey, Identity, InitFinishedValue);
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
					Logger.AllLog(Identity, "Register contol failed.", LogLevel.Error, e);
				}
			}
		}
	}
}
