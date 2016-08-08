using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Extension.Configuration;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Core.Common;
using DotnetSpider.Validation;
using System.Linq;
using StackExchange.Redis;
using DotnetSpider.Core.Monitor;
using System.Net;
using System.Runtime.InteropServices;
using NLog;
using MimeKit;
using MailKit.Net.Smtp;
#if NET_CORE
using System.Text;
#endif

namespace DotnetSpider.Extension
{
	public class ModelSpider
	{
		private const string InitStatusSetName = "init-status";
		private const string ValidateStatusName = "validate-status";
		protected readonly ILogger Logger;
		protected ConnectionMultiplexer Redis;
		protected IDatabase Db;
		protected Spider Spider;
		protected readonly SpiderContext SpiderContext;
		public Action AfterSpiderFinished { get; set; }
		public string Name { get; }

		public ModelSpider(SpiderContext spiderContext)
		{
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(SpiderEnviroment.BaseDirectory, "nlog.config"));
#endif
			SpiderContext = spiderContext;

			if (!SpiderContext.IsBuilt)
			{
				SpiderContext.Build();
			}

			Name = SpiderContext.SpiderName;

			Logger = LogManager.GetCurrentClassLogger();

			InitEnvoriment();
		}

		private void InitEnvoriment()
		{
			if (SpiderContext.Redialer != null)
			{
				if (SpiderContext.Redialer.RedialManager == null)
				{
					SpiderContext.Redialer.RedialManager = new FileRedialManager();
				}
				SpiderContext.Redialer.RedialManager.SetRedialManager(SpiderContext.Redialer.NetworkValidater.GetNetworkValidater(), SpiderContext.Redialer.GetRedialer());
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
				var host = ConfigurationManager.Get("redisHost");

				var confiruation = new ConfigurationOptions()
				{
					ServiceName = "DotnetSpider",
					Password = ConfigurationManager.Get("redisPassword"),
					ConnectTimeout = 65530,
					KeepAlive = 8,
					ConnectRetry = 20,
					SyncTimeout = 65530,
					ResponseTimeout = 65530
				};
#if NET_CORE
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					// Lewis: This is a Workaround for .NET CORE can't use EndPoint to create Socket.
					var address = Dns.GetHostAddressesAsync(host).Result.FirstOrDefault();
					if (address == null)
					{
						throw new SpiderException("Can't resovle your host: " + host);
					}
					confiruation.EndPoints.Add(new IPEndPoint(address, 6379));
				}
				else
				{
					confiruation.EndPoints.Add(new DnsEndPoint(host, 6379));
				}
#else
				confiruation.EndPoints.Add(new DnsEndPoint(host, 6379));
#endif
				Redis = ConnectionMultiplexer.Connect(confiruation);
				Db = Redis.GetDatabase(1);
			}
		}

		public virtual void Run(params string[] args)
		{
			try
			{
				Spider = PrepareSpider(args);

				if (Spider == null)
				{
					return;
				}

				RegisterControl(Spider);

				Spider.Start();

				while (Spider.StatusCode == Status.Running || Spider.StatusCode == Status.Init)
				{
					Thread.Sleep(1000);
				}

				Spider.Dispose();

				AfterSpiderFinished?.Invoke();

				DoValidate();
			}
			finally
			{
				SpiderMonitor.Default.Dispose();
			}
		}

		private void RegisterControl(Spider spider)
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
					Logger.Info(LogInfo.Create("开始数据验证 ...", SpiderContext));

					if (validations != null && validations.Count > 0)
					{
						MailBodyBuilder builder = new MailBodyBuilder(Name, SpiderContext.Validations.Corporation);
						foreach (var validation in validations)
						{
							builder.AddValidateResult(validation.Validate());
						}
						string mailBody = builder.Build();

						var message = new MimeMessage();
						message.From.Add(new MailboxAddress(SpiderContext.Validations.EmailFrom, SpiderContext.Validations.EmailFrom));
						foreach (var address in SpiderContext.Validations.EmailTo.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
						{
							message.To.Add(new MailboxAddress(address, address));
						}

						message.Subject = $"{Name} " + "validation report";

						message.Body = new TextPart("html")
						{
							Text = mailBody
						};

						using (var client = new SmtpClient())
						{
							client.Connect(SpiderContext.Validations.EmailSmtpServer, SpiderContext.Validations.EmailSmtpPort, false);

							// Note: since we don't have an OAuth2 token, disable
							// the XOAUTH2 authentication mechanism.
							client.AuthenticationMechanisms.Remove("XOAUTH2");

							// Note: only needed if the SMTP server requires authentication
							client.Authenticate(SpiderContext.Validations.EmailUser, SpiderContext.Validations.EmailPassword);

							client.Send(message);
							client.Disconnect(true);
						}
					}
				}
				else
				{
					Logger.Info(LogInfo.Create("有其他线程执行了数据验证.", SpiderContext));
				}

				if (needInitStartRequest && Redis != null)
				{
					Db.HashSet(ValidateStatusName, Name, "validate finished");
				}
			}
			catch (Exception e)
			{
				Logger.Error(e, e.Message);
			}
			finally
			{
				if (Redis != null)
				{
					Db.LockRelease(key, 0);
				}
			}
		}

		private Spider PrepareSpider(params string[] args)
		{
			Logger.Info(LogInfo.Create("创建爬虫...", SpiderContext));
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
			var spider = GenerateSpider(SpiderContext.Scheduler.GetScheduler());

			if (args.Contains("rerun"))
			{
				spider.Scheduler.Clear();
				needInitStartRequest = true;
			}

			if (needInitStartRequest)
			{
				PrepareSite();
			}
			Logger.Info(LogInfo.Create("构建内部模块...", SpiderContext));
			SpiderMonitor.Default.Register(spider);
			spider.InitComponent();

			Db?.LockRelease(key, 0);

			return spider;
		}

		private void PrepareSite()
		{
			Logger.Info(LogInfo.Create("准备爬虫数据...", SpiderContext));
			if (SpiderContext.PrepareStartUrls != null)
			{
				foreach (var prepareStartUrl in SpiderContext.PrepareStartUrls)
				{
					prepareStartUrl.Build(SpiderContext.Site, null);
				}
			}

#if !NET_CORE
			if (SpiderContext.CookieThief != null)
			{
				string cookie = SpiderContext.CookieThief.GetCookie();
				if (cookie != "Exception!!!")
				{
					SpiderContext.Site.Cookie = cookie;
				}
			}
#endif
		}

		protected virtual Spider GenerateSpider(IScheduler scheduler)
		{
			EntityProcessor processor = new EntityProcessor(SpiderContext)
			{
				TargetUrlExtractInfos = SpiderContext.TargetUrlExtractInfos?.Select(t => t.GetTargetUrlExtractInfo()).ToList()
			};
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
			var downloader = SpiderContext.Downloader.GetDownloader();
			downloader.Handlers = SpiderContext.Downloader.Handlers;
			spider.SetDownloader(downloader);
			spider.SkipWhenResultIsEmpty = SpiderContext.SkipWhenResultIsEmpty;

			if (SpiderContext.TargetUrlsHandler != null)
			{
				spider.SetCustomizeTargetUrls(SpiderContext.TargetUrlsHandler.Handle);
			}

			return spider;
		}
	}
}
