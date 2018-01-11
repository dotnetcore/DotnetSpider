using System;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using System.Threading;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Redial;
using Polly;
using Polly.Retry;

namespace DotnetSpider.Extension
{
	public abstract class CommonSpider : Spider
	{
		private const string InitFinishedValue = "init complete";
		internal const string InitStatusSetKey = "dotnetspider:init-stats";
		internal string InitLockKey => $"dotnetspider:initLocker:{Identity}";

		protected abstract void MyInit(params string[] arguments);

		public Action DataVerificationAndReport;

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

		public ISpider ToDefaultSpider()
		{
			return new DefaultSpider("", new Site());
		}

		protected override void RunApp(params string[] arguments)
		{
			PrintInfo.Print();

			Logger.Log(Identity, "Build custom component...", Level.Info);

			NetworkCenter.Current.Execute("myInit", () =>
			{
				MyInit(arguments);
			});

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

			CheckIfSettingsCorrect();

			RegisterControl(this);

			base.RunApp(arguments);

			if (IsComplete && DataVerificationAndReport != null)
			{
				NetworkCenter.Current.Execute("verifyAndReport", () =>
				{
					BaseVerification.ProcessVerifidation(Identity, DataVerificationAndReport);
				});
			}
		}

		protected override void InitScheduler(params string[] arguments)
		{
			base.InitScheduler(arguments);

			if (arguments.Contains("rerun"))
			{
				Scheduler.Clear();
				Scheduler.Dispose();
				BaseVerification.RemoveVerifidationLock(Identity);
			}
		}

		/// <summary>
		/// 分布式任务时使用, 只需要调用一次
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		protected override bool IfRequireBuildStartRequests(string[] arguments)
		{
			if (RedisConnection.Default != null)
			{
				if (arguments.Contains("rerun"))
				{
					RedisConnection.Default.Database.HashDelete(InitStatusSetKey, Identity);
					RedisConnection.Default.Database.LockRelease(InitLockKey, 0);
					return true;
				}
				else
				{
					// 如果已经被初始化了, 则不需要再去抢锁了
					var require = IfRequireBuildStartRequests();
					if (!require)
					{
						return false;
					}
					else
					{
						var lockTake = RedisConnection.Default.Database.LockTake(InitLockKey, 0, TimeSpan.FromMinutes(30));
						if (!lockTake)
						{
							while (true)
							{
								var lockerValue = RedisConnection.Default.Database.HashGet(InitStatusSetKey, Identity);
								if (lockerValue != InitFinishedValue)
								{
									Logger.Log(Identity, "Waiting for another crawler inited...", Level.Info);
									Thread.Sleep(1500);
								}
								else
								{
									break;
								}
							}
						}
						return lockTake;
					}
				}
			}
			else
			{
				return true;
			}
		}

		protected override void BuildStartRequestsFinished()
		{
			if (RedisConnection.Default != null)
			{
				bool ifBuildFinished = false;
				for (int i = 0; i < 10; ++i)
				{
					ifBuildFinished = RedisConnection.Default.Database.HashSet(InitStatusSetKey, Identity, InitFinishedValue);
					if (ifBuildFinished)
					{
						break;
					}
					else
					{
						Thread.Sleep(1000);
					}
				}
				if (!ifBuildFinished)
				{
					var msg = "Init status set failed.";
					Logger.Log(Identity, msg, Level.Error);
					throw new SpiderException(msg);
				}

				bool ifRemoveInitLocker = false;
				for (int i = 0; i < 10; ++i)
				{
					ifRemoveInitLocker = RedisConnection.Default.Database.KeyDelete(InitLockKey);
					if (ifRemoveInitLocker)
					{
						break;
					}
					else
					{
						Thread.Sleep(1000);
					}
				}
				if (!ifRemoveInitLocker)
				{
					var msg = "Remove init locker failed.";
					Logger.Log(Identity, msg, Level.Error);
					throw new SpiderException(msg);
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
					Logger.Log(Identity, "Register contol failed.", Level.Error, e);
				}
			}
		}

		private bool IfRequireBuildStartRequests()
		{
			return RedisConnection.Default.Database.HashGet(InitStatusSetKey, Identity) != InitFinishedValue;
		}
	}
}
