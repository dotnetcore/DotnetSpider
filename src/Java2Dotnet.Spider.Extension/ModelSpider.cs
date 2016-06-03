using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Monitor;
using Java2Dotnet.Spider.Extension.ORM;
using Java2Dotnet.Spider.Extension.Pipeline;
using Java2Dotnet.Spider.Extension.Processor;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial;
using Java2Dotnet.Spider.Redial.NetworkValidater;
using Java2Dotnet.Spider.Redial.RedialManager;
using Java2Dotnet.Spider.Validation;
using Java2Dotnet.Spider.JLog;
using System.Linq;
using RedisSharp;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if NET_45
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
#endif

namespace Java2Dotnet.Spider.Extension
{
	public class ModelSpider
	{
		private const string InitStatusSetName = "init-status";
		private const string ValidateStatusName = "validate-status";
		protected readonly ILog Logger;

		protected readonly SpiderContext SpiderContext;
		public string Name { get; }
		protected Core.Spider spider = null;

		public ModelSpider(SpiderContext spiderContext)
		{

#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
			SpiderContext = spiderContext;

			if (!SpiderContext.IsBuilt)
			{
				SpiderContext.Build();
			}

			Name = $"{SpiderContext.UserId}-{SpiderContext.SpiderName}";

			Logger = LogUtils.GetLogger(SpiderContext.SpiderName, SpiderContext.UserId, SpiderContext.TaskGroup);

			InitEnvoriment();
		}

		private void InitEnvoriment()
		{
			if (SpiderContext.Redialer != null)
			{
				RedialManagerUtils.RedialManager = new RedisRedialManager(Logger);

				RedialManagerUtils.RedialManager.NetworkValidater = GetNetworValidater(SpiderContext.NetworkValidater);
				RedialManagerUtils.RedialManager.Redialer = SpiderContext.Redialer.GetRedialer();
			}

			if (SpiderContext.Downloader == null)
			{
				SpiderContext.Downloader = new HttpDownloader();
			}

			if (SpiderContext.Site == null)
			{
				SpiderContext.Site = new Site();
			}
		}

		public virtual void Run(params string[] args)
		{

			try
			{
				spider = PrepareSpider(args);
				if (spider == null)
				{
					return;
				}

				var redisScheduler = spider.Scheduler as Scheduler.RedisScheduler;
				if (redisScheduler != null)
				{
					try
					{
						redisScheduler.Redis.Subscribe($"{spider.Identity}", (c, m) =>
						{
							switch (m)
							{
								case "stop":
									{
										spider.Stop();
										break;
									}
								case "start":
									{
										spider.Start();
										break;
									}
								case "exit":
									{
										spider.Exit();
										break;
									}
							}
						});
					}
					catch
					{
						// ignored
					}
				}

				spider.Start();

				while (spider.StatusCode == Status.Stopped || spider.StatusCode == Status.Running || spider.StatusCode == Status.Init)
				{
					Thread.Sleep(1000);
				}

				spider?.Dispose();

				RunAfterSpiderFinished();

				DoValidate();
			}
			finally
			{
				Log.WaitForExit();
			}
		}

		public void Clear()
		{
			var redisScheduler = spider.Scheduler as Scheduler.RedisScheduler;
			if (redisScheduler != null)
			{
				redisScheduler.Clear(spider);
			}
		}

		private void DoValidate()
		{
			RedisServer redis = new RedisServer(ConfigurationManager.Get("redisHost"), 6379, ConfigurationManager.Get("redisPassword"));

			string key = "locker-validate-" + Name;
			try
			{
				if (SpiderContext.Validations == null)
				{
					return;
				}

				var validations = SpiderContext.Validations.GetValidations();

				if (validations != null && validations.Count > 0)
				{
					foreach (var validation in validations)
					{
						validation.CheckArguments();
					}
				}

				if (redis != null)
				{
					while (!redis.LockTake(key, "0", TimeSpan.FromMinutes(10)))
					{
						Thread.Sleep(1000);
					}
				}

				var lockerValue = redis?.HashGet(ValidateStatusName, Name);
				bool needInitStartRequest = lockerValue != "validate finished";

				if (needInitStartRequest)
				{
					Logger.Info("开始数据验证 ...");

					if (validations != null && validations.Count > 0)
					{
						MailBodyBuilder builder = new MailBodyBuilder(Name, SpiderContext.Validations.Corporation);
						foreach (var validation in validations)
						{
							builder.AddValidateResult(validation.Validate());
						}
						string mailBody = builder.Build();

						using (EmailClient client = new EmailClient(SpiderContext.Validations.EmailSmtpServer, SpiderContext.Validations.EmailUser, SpiderContext.Validations.EmailPassword, SpiderContext.Validations.EmailSmtpPort))
						{
							client.SendMail(new EmaillMessage($"{Name} " + "validation report", mailBody, SpiderContext.Validations.EmailTo) { IsHtml = true });
						}
					}
				}
				else
				{
					Logger.Info("有其他线程执行了数据验证.");
				}

				if (needInitStartRequest)
				{
					redis?.HashSet(ValidateStatusName, Name, "validate finished");
				}
			}
			catch (Exception e)
			{
				Logger.Error(e.Message, e);
			}
			finally
			{
				redis?.LockRelease(key, 0);
			}
		}

		private Core.Spider PrepareSpider(params string[] args)
		{
			RedisServer redis = GetManageRedisServer();

			var schedulerType = SpiderContext.Scheduler.Type;
			bool isTestSpider = args != null && args.Contains("test");

			switch (schedulerType)
			{
				case Configuration.Scheduler.Types.Queue:
					{
						PrepareSite();
						var spider = GenerateSpider(SpiderContext.Scheduler.GetScheduler());
						if (isTestSpider && spider.Site.StartRequests.Count > 0)
						{
							spider.Site.StartRequests = new List<Request> { spider.Site.StartRequests[0] };
						}
						spider.InitComponent();
						return spider;
					}
				case Configuration.Scheduler.Types.Redis:
					{
						var scheduler = (Scheduler.RedisScheduler)(SpiderContext.Scheduler.GetScheduler());

						string key = "locker-" + Name;
						if (args != null && args.Length > 0)
						{
							if (args.Contains("rerun"))
							{
								if (redis != null)
								{
									redis.KeyDelete(key);
									redis.HashDelete("init-status", Name);
									redis.HashDelete("validate-status", Name);
									redis.HashDelete(Scheduler.RedisScheduler.TaskStatus, Name);
									redis.SortedSetRemove(Scheduler.RedisScheduler.TaskList, Name);
								}

								scheduler.Redis.KeyDelete(Scheduler.RedisScheduler.GetQueueKey(Name));
								scheduler.Redis.KeyDelete(Scheduler.RedisScheduler.GetSetKey(Name));
								scheduler.Redis.KeyDelete(Scheduler.RedisScheduler.GetItemKey(Name));
							}
							if (args.Contains("noconsole"))
							{
								Log.WriteLine("No console log info.");
								Log.NoConsole = true;
							}
						}

						try
						{
							if (redis != null)
							{
								while (!redis.LockTake(key, "0", TimeSpan.FromMinutes(10)))
								{
									Thread.Sleep(1000);
								}
							}

							var lockerValue = redis?.HashGet(InitStatusSetName, Name);
							bool needInitStartRequest = lockerValue != "init finished";

							if (needInitStartRequest)
							{
								PrepareSite();
							}
							else
							{
								Logger.Info("Site 已经初始化");
								SpiderContext.Site.ClearStartRequests();
							}

							Logger.Info("创建爬虫...");

							var spider = GenerateSpider(scheduler);

							spider.SaveStatus = true;
							SpiderMonitor.Default.Register(spider);

							Logger.Info("构建内部模块...");

							if (isTestSpider && spider.Site.StartRequests.Count > 0)
							{
								spider.Site.StartRequests = new List<Request> { spider.Site.StartRequests[0] };
							}

							spider.InitComponent();

							if (needInitStartRequest)
							{
								redis?.HashSet(InitStatusSetName, Name, "init finished");
							}

							return spider;
						}
						catch (Exception e)
						{
							Logger.Error(e.Message, e);
							return null;
						}
						finally
						{
							try
							{
								redis?.LockRelease(key, 0);
							}
							catch
							{
								// ignored
							}
						}
					}
			}

			throw new SpiderExceptoin("初始化失败.");
		}

		private RedisServer GetManageRedisServer()
		{
			var host = ConfigurationManager.Get("redisHost");
			var portStr = ConfigurationManager.Get("redisPort");
			if (string.IsNullOrEmpty(host))
			{
				return null;
			}
			int port = string.IsNullOrEmpty(portStr) ? 6379 : int.Parse(portStr);
			return new RedisServer(host, port, ConfigurationManager.Get("redisPassword"));
		}

		private void PrepareSite()
		{
			if (SpiderContext.PrepareStartUrls != null)
			{
				foreach (var prepareStartUrl in SpiderContext.PrepareStartUrls)
				{
					prepareStartUrl.Build(SpiderContext.Site, null);
				}
			}

#if !NET_CORE
			if (SpiderContext.GetCookie != null)
			{
				string cookie = SpiderContext.GetCookie.GetCookie();
				if (cookie != "Exception!!!")
				{
					SpiderContext.Site.Cookie = cookie;
				}
			}
#endif
		}

		protected virtual Core.Spider GenerateSpider(IScheduler scheduler)
		{
			EntityProcessor processor = new EntityProcessor(SpiderContext);
			processor.TargetUrlExtractInfos = SpiderContext.TargetUrlExtractInfos?.Select(t => t.GetTargetUrlExtractInfo()).ToList();
			foreach (var entity in SpiderContext.Entities)
			{
				processor.AddEntity(entity);
			}

			EntityGeneralSpider spider = new EntityGeneralSpider(SpiderContext.Site, Name, SpiderContext.UserId, SpiderContext.TaskGroup, processor, scheduler);

			foreach (var entity in SpiderContext.Entities)
			{
				string entiyName = entity.Identity;

				var schema = entity.Schema;

				List<IEntityPipeline> pipelines = new List<IEntityPipeline>();
				foreach (var pipeline in SpiderContext.Pipelines)
				{
					pipelines.Add(pipeline.GetPipeline(schema, entity));
				}
				spider.AddPipeline(new EntityPipeline(entiyName, pipelines));
			}
			spider.SetCachedSize(SpiderContext.CachedSize);
			spider.SetEmptySleepTime(SpiderContext.EmptySleepTime);
			spider.SetThreadNum(SpiderContext.ThreadNum);
			spider.Deep = SpiderContext.Deep;
			spider.SetDownloader(SpiderContext.Downloader.GetDownloader());
			spider.SkipWhenResultIsEmpty = SpiderContext.SkipWhenResultIsEmpty;
			if (SpiderContext.PageHandlers != null)
			{
				spider.PageHandlers = new List<Action<Page>>();
				foreach (var pageHandler in SpiderContext.PageHandlers)
				{
					spider.PageHandlers.Add(pageHandler.Customize);
				}
			}

			if (SpiderContext.TargetUrlsHandler != null)
			{
				spider.SetCustomizeTargetUrls(SpiderContext.TargetUrlsHandler.Handle);
			}

			return spider;
		}

		protected void RunAfterSpiderFinished()
		{
		}

		private INetworkValidater GetNetworValidater(NetworkValidater networkValidater)
		{
			switch (networkValidater.Type)
			{
				case NetworkValidater.Types.Vps:
					{
						return new Redial.NetworkValidater.VpsNetworkValidater(((Configuration.VpsNetworkValidater)networkValidater).InterfaceNum);
					}
				case NetworkValidater.Types.Defalut:
					{
						return new Redial.NetworkValidater.DefaultNetworkValidater();
					}
				case NetworkValidater.Types.Vpn:
					{
						return new Redial.NetworkValidater.VpnNetworkValidater(((Configuration.VpnNetworkValidater)networkValidater).VpnName);
					}
			}
			return null;
		}
	}
}
