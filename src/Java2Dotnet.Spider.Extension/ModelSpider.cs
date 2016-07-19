using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.ORM;
using Java2Dotnet.Spider.Extension.Pipeline;
using Java2Dotnet.Spider.Extension.Processor;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial;
using Java2Dotnet.Spider.Redial.NetworkValidater;
using Java2Dotnet.Spider.Redial.RedialManager;
using Java2Dotnet.Spider.Validation;
using Java2Dotnet.Spider.Log;
using System.Linq;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using Java2Dotnet.Spider.Core.Monitor;
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
		protected readonly ILogService Logger;
		protected ConnectionMultiplexer Redis;
		protected IDatabase Db;
		protected Core.Spider spider;
		protected readonly SpiderContext SpiderContext;
		public Action AfterSpiderFinished { get; set; }
		public string Name { get; }

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

			Logger = new Logger(SpiderContext.SpiderName, SpiderContext.UserId, SpiderContext.TaskGroup);

			InitEnvoriment();
		}

		private void InitEnvoriment()
		{
			if (SpiderContext.Redialer != null)
			{
				if (Db != null)
				{
					RedialManagerUtils.RedialManager = new RedisRedialManager(Db, SpiderContext.Redialer.NetworkValidater.GetNetworkValidater(), SpiderContext.Redialer.GetRedialer(), Logger);
				}
				else
				{
					RedialManagerUtils.RedialManager = FileLockerRedialManager.Default;
					RedialManagerUtils.RedialManager.Logger = Logger;
					RedialManagerUtils.RedialManager.NetworkValidater = SpiderContext.Redialer.NetworkValidater.GetNetworkValidater();
					RedialManagerUtils.RedialManager.Redialer = SpiderContext.Redialer.GetRedialer();
				}
			}

			if (SpiderContext.Downloader == null)
			{
				SpiderContext.Downloader = new HttpDownloader();
			}

			if (SpiderContext.Site == null)
			{
				SpiderContext.Site = new Site();
			}
			if (!string.IsNullOrEmpty(ConfigurationManager.Get("redisHost")) && string.IsNullOrWhiteSpace(ConfigurationManager.Get("redisHost")))
			{
				Redis = ConnectionMultiplexer.Connect(new ConfigurationOptions()
				{
					ServiceName = "DotnetSpider",
					Password = ConfigurationManager.Get("redisPassword"),
					ConnectTimeout = 5000,
					KeepAlive = 8,
					EndPoints =
				{ ConfigurationManager.Get("redisHost"), "6379" }
				});
				Db = Redis.GetDatabase(1);
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

				RegisterControl(spider);

				spider.Start();

				while (spider.StatusCode == Status.Running || spider.StatusCode == Status.Init)
				{
					Thread.Sleep(1000);
				}

				spider.Dispose();

				AfterSpiderFinished?.Invoke();

				DoValidate();
			}
			finally
			{
				SpiderMonitor.Default.Dispose();
			}
		}

		private void RegisterControl(Core.Spider spider)
		{
			if (Redis != null)
			{
				try
				{
					Redis.GetSubscriber().Subscribe($"{spider.Identity}", (c, m) =>
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
		}

		private void DoValidate()
		{
			if (SpiderContext.Validations == null)
			{
				return;
			}

			string key = "locker-validate-" + Name;

			try
			{
				var validations = SpiderContext.Validations.GetValidations();

				if (validations != null && validations.Count > 0)
				{
					foreach (var validation in validations)
					{
						validation.CheckArguments();
					}
				}
				bool needInitStartRequest = true;
				if (Redis != null)
				{
					while (!Db.LockTake(key, "0", TimeSpan.FromMinutes(10)))
					{
						Thread.Sleep(1000);
					}

					var lockerValue = Db.HashGet(ValidateStatusName, Name);
					needInitStartRequest = lockerValue != "validate finished";
				}
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

				if (needInitStartRequest && Redis != null)
				{
					Db.HashSet(ValidateStatusName, Name, "validate finished");
				}
			}
			catch (Exception e)
			{
				Logger.Error(e.Message, e);
			}
			finally
			{
				if (Redis != null)
				{
					Db.LockRelease(key, 0);
				}
			}
		}

		private Core.Spider PrepareSpider(params string[] args)
		{
			Logger.Info("创建爬虫...");
			bool needInitStartRequest = true;
			string key = "locker-" + Name;
			if (Db != null)
			{
				while (!Db.LockTake(key, "0", TimeSpan.FromMinutes(10)))
				{
					Thread.Sleep(1000);
				}
				var lockerValue = Db.HashGet(InitStatusSetName, Name);
				needInitStartRequest = lockerValue != "init finished";
			}

			if (needInitStartRequest)
			{
				PrepareSite();
			}

			var spider = GenerateSpider(SpiderContext.Scheduler.GetScheduler());
			Logger.Info("构建内部模块...");
			SpiderMonitor.Default.Register(spider);
			spider.InitComponent();

			if (Db != null)
			{
				Db.LockRelease(key, 0);
			}

			return spider;
		}

		private void PrepareSite()
		{
			Logger.Info("准备爬虫数据...");
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
				string entiyName = entity.Name;

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
#if !NET_CORE
						return new Redial.NetworkValidater.VpnNetworkValidater(((Configuration.VpnNetworkValidater)networkValidater).VpnName);
#else
						throw new SpiderExceptoin("unsport vpn redial on linux.");
#endif
					}
			}
			return null;
		}
	}
}
