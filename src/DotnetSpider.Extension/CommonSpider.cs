using System;
using System.Linq;
using DotnetSpider.Core;
using System.Threading;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Redial;

namespace DotnetSpider.Extension
{
	/// <summary>
	/// 通用爬虫
	/// </summary>
	public abstract class CommonSpider : Spider
	{
		/// <summary>
		/// 验证结果保存到Redis中的Key
		/// </summary>
		private const string ValidateStatusKey = "dotnetspider:validate-stats";
		private const string InitFinishedValue = "init complete";
		internal const string InitStatusSetKey = "dotnetspider:init-stats";
		internal string InitLockKey => $"dotnetspider:initLocker:{Identity}";

		/// <summary>
		/// 自定义的初始化
		/// </summary>
		/// <param name="arguments">运行参数</param>
		protected abstract void MyInit(params string[] arguments);

		/// <summary>
		/// 爬虫结束后, 执行的数据验证和报告
		/// </summary>
		public Action DataVerificationAndReport;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="site">站点信息</param>
		public CommonSpider(Site site) : base(site)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="name">名称</param>
		/// <param name="site">站点信息</param>
		public CommonSpider(string name, Site site) : base(site)
		{
			Name = name;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="name">名称</param>
		public CommonSpider(string name) : base(new Site())
		{
			Name = name;
		}

		/// <summary>
		/// 运行爬虫
		/// </summary>
		/// <param name="arguments">运行参数</param>
		protected override void Execute(params string[] arguments)
		{
			PrintInfo.Print();

			Logger.Information("Build custom component...");

			NetworkCenter.Current.Execute("myInit", () =>
			{
				MyInit(arguments);
			});

			if (arguments.Contains("skip"))
			{
				EmptySleepTime = 1000;

				if (Pipelines == null || Pipelines.Count == 0)
				{
					AddPipeline(new ConsolePipeline());
				}
				if (_pageProcessors == null || _pageProcessors.Count == 0)
				{
					AddPageProcessors(new NullPageProcessor());
				}
			}

			ValidateSettings();

			RegisterControl(this);

			base.Execute(arguments);

			if (IsCompleted && DataVerificationAndReport != null)
			{
				ProcessVerifidation();
			}
		}

		/// <summary>
		/// 初始化队列
		/// </summary>
		/// <param name="arguments">运行参数</param>
		protected override void InitScheduler(params string[] arguments)
		{
			base.InitScheduler(arguments);

			if (arguments.Contains("rerun"))
			{
				Scheduler.Dispose();
				RemoveVerifidationLock();
			}
		}

		/// <summary>
		/// 分布式任务时使用, 只需要调用一次
		/// </summary>
		/// <param name="arguments">运行参数</param>
		/// <returns>是否需要运行起始链接构造器</returns>
		protected override bool IfRequireBuildStartUrlsBuilders(string[] arguments)
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
		protected override void BuildStartUrlsBuildersCompleted()
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

		/// <summary>
		/// 删除验证的锁, 让其它爬虫节点再次验证
		/// </summary>
		protected void RemoveVerifidationLock()
		{
			RedisConnection.Default?.Database.HashDelete(ValidateStatusKey, Identity);
		}

		private bool IfRequireBuildStartRequests()
		{
			return RedisConnection.Default.Database.HashGet(InitStatusSetKey, Identity) != InitFinishedValue;
		}

		/// <summary>
		/// 执行数据验证
		/// </summary>
		private void ProcessVerifidation()
		{
			NetworkCenter.Current.Execute("verifyAndReport", () =>
			{
				string key = $"dotnetspider:validateLocker:{Identity}";

				try
				{
					bool needVerify = true;
					if (RedisConnection.Default != null)
					{
						while (!RedisConnection.Default.Database.LockTake(key, "0", TimeSpan.FromMinutes(10)))
						{
							Thread.Sleep(1000);
						}

						var lockerValue = RedisConnection.Default.Database.HashGet(ValidateStatusKey, Identity);
						needVerify = lockerValue != "verify completed.";
					}
					if (needVerify)
					{
						Logger.Information("Start data verification...");
						DataVerificationAndReport();
						Logger.Information("Data verification complete.");
					}
					else
					{
						Logger.Information("Data verification is done already.");
					}

					if (needVerify)
					{
						RedisConnection.Default?.Database.HashSet(ValidateStatusKey, Identity, "verify completed.");
					}
				}
				catch (Exception e)
				{
					Logger.Error(e.Message);
				}
				finally
				{
					RedisConnection.Default?.Database.LockRelease(key, 0);
				}
			});
		}
	}
}
