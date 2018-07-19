using DotnetSpider.Downloader.Redial;
using DotnetSpider.Downloader.Redial.Redialer;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DotnetSpider.Downloader.Test
{
	public class RedialTest
	{
		[Fact(DisplayName = "ConfigFileMissing")]
		public void ConfigFileMissing()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				new MutexRedialExecutor(new DefaultAdslRedialer("adsl_account_null.txt"));
			});
		}

		[Fact(DisplayName = "MulitThread")]
		public void MulitThread()
		{
			PrepareAdslConfig();
			var executor = new MutexRedialExecutor(new DefaultAdslRedialer("adsl_account.txt"));
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

			var executor2 = new FileLockerRedialExecutor(new DefaultAdslRedialer("adsl_account.txt"));
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
