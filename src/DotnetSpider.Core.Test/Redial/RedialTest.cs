using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Redial;
using DotnetSpider.Core.Redial.Redialer;
using DotnetSpider.Core.Scheduler;
using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DotnetSpider.Core.Test.Redial
{

	public class RedialTest
	{
		[Fact(DisplayName = "Setting")]
		public void Setting()
		{
			var site = new Site { EncodingName = "UTF-8" };

			// Set start/seed url
			site.AddStartUrl("http://www.cnblogs.com/");

			Spider spider = Spider.Create(site,
				// crawler identity
				"cnblogs_" + DateTime.Now.ToString("yyyyMMddhhmmss"),
				// use memoery queue scheduler
				new QueueDuplicateRemovedScheduler(),
				// default page processor will save whole html, and extract urls to target urls via regex
				new DefaultPageProcessor(new[] { "cnblogs\\.com" }))
				// save crawler result to file in the folder: \{running directory}\data\{crawler identity}\{guid}.dsd
				.AddPipeline(new FilePipeline());
			PrepareAdslConfig();
			spider.RedialExecutor = new MutexRedialExecutor(new AdslRedialer("adsl_account.txt"));
			Assert.NotNull(spider.RedialExecutor);
		}

		[Fact(DisplayName = "ConfigFileMissing")]
		public void ConfigFileMissing()
		{
			Assert.Throws<SpiderException>(() =>
			{
				new MutexRedialExecutor(new AdslRedialer("adsl_account_null.txt"));
			});
		}

		[Fact(DisplayName = "MulitThread")]
		public void MulitThread()
		{
			PrepareAdslConfig();
			var executor = new MutexRedialExecutor(new AdslRedialer("adsl_account.txt"));
			executor.IsTest = true;
			NetworkCenter.Current.Executor = executor;

			Stopwatch watch = new Stopwatch();
			watch.Start();

			Parallel.For(0, 5, new ParallelOptions
			{
				MaxDegreeOfParallelism = 5
			}, j =>
			{
				for (int i = 0; i < 400; ++i)
				{
					NetworkCenter.Current.Executor.Execute("test", () =>
					{
						Console.Write("requested,");
					});

					if (i % 100 == 0)
					{
						NetworkCenter.Current.Executor.Redial();
					}
				}
			});
			watch.Stop();
			Console.WriteLine("cost: " + watch.ElapsedMilliseconds);

			var executor2 = new FileLockerRedialExecutor(new AdslRedialer("adsl_account.txt"));
			executor2.IsTest = true;
			NetworkCenter.Current.Executor = executor2;
			watch.Reset();
			watch.Start();
			Parallel.For(0, 5, new ParallelOptions
			{
				MaxDegreeOfParallelism = 5
			}, j =>
			{
				for (int i = 0; i < 400; ++i)
				{
					NetworkCenter.Current.Executor.Execute("test", () =>
					{
						Console.Write("requested,");
					});

					if (i % 100 == 0)
					{
						NetworkCenter.Current.Executor.Redial();
					}
				}
			});
			watch.Stop();
			Console.WriteLine("cost: " + watch.ElapsedMilliseconds);
		}

		private void PrepareAdslConfig()
		{
			FileInfo file = new FileInfo("adsl_account.txt");
			if (!file.Exists)
			{
				File.AppendAllLines("adsl_account.txt", new[] { "a", "b", "c" });
			}
		}
	}
}
