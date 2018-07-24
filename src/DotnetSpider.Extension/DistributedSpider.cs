using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Extension.Infrastructure;
using System;
using System.Linq;
using System.Threading;

namespace DotnetSpider.Extension
{
	/// <summary>
	/// 分布式爬虫
	/// </summary>
	public abstract class DistributedSpider : Spider
	{
		/// <summary>
		/// 验证结果保存到Redis中的Key
		/// </summary>
		private const string ValidateStatusKey = "dotnetspider:validate-stats";
		private const string InitFinishedValue = "init complete";
		internal const string InitStatusSetKey = "dotnetspider:init-stats";
		internal string InitLockKey => $"dotnetspider:initLocker:{Identity}";

		/// <summary>
		/// 构造方法
		/// </summary>
		public DistributedSpider() : this(new Site())
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="site">目标站点信息</param>
		public DistributedSpider(Site site) : base(site)
		{
		}

		/// <summary>
		/// 运行爬虫
		/// </summary>
		/// <param name="arguments">运行参数</param>
		protected override void Execute(params string[] arguments)
		{
			RegisterControl(this);

			base.Execute(arguments);
		}

		protected override bool IfRequireVerifyDataOrGenerateReport(string[] arguments)
		{
			var factor = arguments.Any(t => t?.ToLower() == SpiderArguments.ExcludeRequestBuilder) || IsCompleted;

			if (factor)
			{
				string key = $"dotnetspider:validateLocker:{Identity}";
				if (RedisConnection.Default != null)
				{
					while (!RedisConnection.Default.Database.LockTake(key, "0", TimeSpan.FromMinutes(10)))
					{
						Thread.Sleep(1000);
					}

					var lockerValue = RedisConnection.Default.Database.HashGet(ValidateStatusKey, Identity);
					factor = lockerValue != "verify completed.";
				}
				if (!factor)
				{
					Logger.Information("Data verification is done already.");
				}
				if (factor)
				{
					RedisConnection.Default?.Database.HashSet(ValidateStatusKey, Identity, "verify completed.");
				}
			}

			return factor;
		}

		protected override void AfterVerifyDataOrGenerateReport()
		{
			string key = $"dotnetspider:validateLocker:{Identity}";
			base.AfterVerifyDataOrGenerateReport();
			RedisConnection.Default?.Database.LockRelease(key, 0);
		}

		/// <summary>
		/// 初始化队列
		/// </summary>
		protected override void ResetScheduler()
		{
			// 删除验证的锁, 让爬虫可以再次验证
			RedisConnection.Default?.Database.HashDelete(ValidateStatusKey, Identity);
		}

		/// <summary>
		/// 分布式任务时使用, 只需要调用一次
		/// </summary>
		/// <param name="arguments">运行参数</param>
		/// <returns>是否需要运行起始链接构造器</returns>
		protected override bool IfRequireRunRequestBuilders(string[] arguments)
		{
			if (RedisConnection.Default != null)
			{
				if (arguments.Any(a => a?.ToLower() == SpiderArguments.Reset))
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
									Logger.Information("Waiting for another crawler inited...");
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

		/// <summary>
		/// 初始化起始链结束后的解锁, 分布式任务时解锁成功则其它爬虫会结束等待状态, 一起进入运行状态
		/// </summary>
		protected override void RunStartRequestBuildersCompleted()
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
					var msg = "Init status set failed";
					Logger.Error(msg);
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
					var msg = "Remove init locker failed";
					Logger.Error(msg);
					throw new SpiderException(msg);
				}
			}
		}

		/// <summary>
		/// 订阅 Redis的消息队列, 实现消息队列对爬虫的控制
		/// </summary>
		/// <param name="spider">爬虫</param>
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
					Logger.Error($"Register contol failed：{e}.");
				}
			}
		}

		private bool IfRequireBuildStartRequests()
		{
			return RedisConnection.Default.Database.HashGet(InitStatusSetKey, Identity) != InitFinishedValue;
		}
	}
}
