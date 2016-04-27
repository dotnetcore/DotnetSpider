using System;
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

namespace Java2Dotnet.Spider.Extension
{
	public class ContextSpider
	{
		private const string InitStatusSetName = "init-status";
		private const string ValidateStatusName = "validate-status";
		protected static readonly ILog Logger = LogManager.GetLogger();

		protected readonly SpiderContext SpiderContext;
		private RedisServer _redis;
		public string Name { get; }

		public ContextSpider(SpiderContext spiderContext)
		{
			SpiderContext = spiderContext;

			Name = SpiderContext.SpiderName;

			InitEnvoriment();
		}

		private void InitEnvoriment()
		{
			_redis = new RedisServer(ConfigurationManager.Get("redisHost"), 6379, ConfigurationManager.Get("redisPassword"));

			if (SpiderContext.Redialer != null)
			{
				//RedialManagerUtils.RedialManager = FileLockerRedialManager.Default;
				RedialManagerUtils.RedialManager = new RedisRedialManager();

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
			Core.Spider spider = null;
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
						redisScheduler.Redis.Subscribe($"{Log.UserId}-{spider.Identity}", (c, m) =>
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

				RunAfterSpiderFinished();

				DoValidate();
			}
			finally
			{
				spider?.Dispose();

				Log.WaitForExit();
			}
		}

		private void DoValidate()
		{
			_redis = new RedisServer(ConfigurationManager.Get("redisHost"), 6379, ConfigurationManager.Get("redisPassword"));

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

				Logger.Info($"Lock: {key} to keep only one validate process.");

				while (!_redis.LockTake(key, "0", TimeSpan.FromMinutes(10)))
				{
					Thread.Sleep(1000);
				}

				var lockerValue = _redis.HashGet(ValidateStatusName, Name);
				bool needInitStartRequest = lockerValue != "validate finished";

				if (needInitStartRequest)
				{
					Logger.Info("Start validate ...");

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
					Logger.Info("No need to validate on this process because other process did.");
				}

				if (needInitStartRequest)
				{
					_redis.HashSet(ValidateStatusName, Name, "validate finished");
				}
			}
			catch (Exception e)
			{
				Logger.Error(e.Message, e);
			}
			finally
			{
				Logger.Info("Release locker.");

				_redis.LockRelease(key, 0);
			}
		}

		private Core.Spider PrepareSpider(params string[] args)
		{
			Logger.Info($"Spider Name Md5Encrypt: {Encrypt.Md5Encrypt(Name)}");

			var schedulerType = SpiderContext.Scheduler.Type;

			switch (schedulerType)
			{
				case Configuration.Scheduler.Types.Queue:
					{
						PrepareSite();
						var spider = GenerateSpider(SpiderContext.Scheduler.GetScheduler());
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
								Logger.Info($"Starting execute command: rerun");

								_redis.KeyDelete(Scheduler.RedisScheduler.GetQueueKey(Name));
								_redis.KeyDelete(Scheduler.RedisScheduler.GetSetKey(Name));
								_redis.HashDelete(Scheduler.RedisScheduler.TaskStatus, Name);
								_redis.KeyDelete(Scheduler.RedisScheduler.ItemPrefix + Name);
								_redis.KeyDelete(Name);
								_redis.KeyDelete(key);
								_redis.SortedSetRemove(Scheduler.RedisScheduler.TaskList, Name);
								_redis.HashDelete("init-status", Name);
								_redis.HashDelete("validate-status", Name);
								_redis.KeyDelete("set-" + Encrypt.Md5Encrypt(Name));
								Logger.Info($"Execute command: rerun finished.");
							}
							if (args.Contains("noconsole"))
							{
								Log.WriteLine("No console log info.");
								Log.NoConsole = true;
							}
						}

						try
						{
							Logger.Info($"Lock: {key} to keep only one prepare process.");
							while (!_redis.LockTake(key, "0", TimeSpan.FromMinutes(10)))
							{
								Thread.Sleep(1000);
							}

							var lockerValue = _redis.HashGet(InitStatusSetName, Name);
							bool needInitStartRequest = lockerValue != "init finished";

							if (needInitStartRequest)
							{
								Logger.Info("Preparing site...");

								PrepareSite();
							}
							else
							{
								Logger.Info("No need to prepare site because other process did it.");
								SpiderContext.Site.ClearStartRequests();
							}

							Logger.Info("Start creating Spider...");

							var spider = GenerateSpider(scheduler);

							Logger.Info("Creat spider finished.");

							spider.SaveStatus = true;
							SpiderMonitor.Default.Register(spider);

							Logger.Info("Start init component...");
							spider.InitComponent();
							Logger.Info("Init component finished.");

							if (needInitStartRequest)
							{
								_redis.HashSet(InitStatusSetName, Name, "init finished");
							}

							Logger.Info("Creating Spider finished.");

							return spider;
						}
						catch (Exception e)
						{
							Logger.Error(e.Message, e);
							return null;
						}
						finally
						{
							Logger.Info("Release locker.");
							try
							{
								_redis.LockRelease(key, 0);
							}
							catch
							{
								// ignored
							}
						}
					}
			}

			throw new SpiderExceptoin("Prepare spider failed.");
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
		}

		protected virtual Core.Spider GenerateSpider(IScheduler scheduler)
		{
			EntityProcessor processor = new EntityProcessor(SpiderContext);
			foreach (var entity in SpiderContext.Entities)
			{
				processor.AddEntity(entity);
			}

			EntityGeneralSpider spider = new EntityGeneralSpider(SpiderContext.SpiderName, processor, scheduler);

			foreach (var entity in SpiderContext.Entities)
			{
				string entiyName = entity.SelectToken("$.Identity")?.ToString();

				var schema = entity.SelectToken("$.Schema")?.ToObject<Schema>();

				switch (SpiderContext.Pipeline.Type)
				{
					case Configuration.Pipeline.Types.MongoDb:
						{
							spider.AddPipeline(new EntityPipeline(entiyName, SpiderContext.Pipeline.GetPipeline(schema, entity)));
							break;
						}
					case Configuration.Pipeline.Types.MySql:
						{
							spider.AddPipeline(new EntityPipeline(entiyName, SpiderContext.Pipeline.GetPipeline(schema, entity)));
							break;
						}
					case Configuration.Pipeline.Types.MySqlFile:
						{
							spider.AddPipeline(new EntityPipeline(entiyName, SpiderContext.Pipeline.GetPipeline(schema, entity)));
							break;
						}
				}
			}
			spider.SetCachedSize(SpiderContext.CachedSize);
			spider.SetEmptySleepTime(SpiderContext.EmptySleepTime);
			spider.SetThreadNum(SpiderContext.ThreadNum);
			spider.Deep = SpiderContext.Deep;
			spider.SetDownloader(SpiderContext.Downloader.GetDownloader());

			if (SpiderContext.PageHandler != null)
			{
				spider.PageHandler = SpiderContext.PageHandler.Customize;
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
			}
			return null;
		}
	}
}
