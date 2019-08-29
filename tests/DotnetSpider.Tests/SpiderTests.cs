using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.Scheduler;
using DotnetSpider.Statistics.Store;
using Microsoft.Extensions.Configuration;
using Serilog;
using Xunit;

namespace DotnetSpider.Tests
{
	public class SpiderTests : TestBase
	{
		public class SqlServerStorageOptions : SpiderOptions
		{
			public override string StorageConnectionString => "ConnectionString";
			public override string Storage => "DotnetSpider.DataFlow.Storage.SqlServerEntityStorage,DotnetSpider";
			public override StorageType StorageType => StorageType.InsertAndUpdate;

			public override bool StorageIgnoreCase => false;
			public override int StorageRetryTimes => 800;
			public override bool StorageUseTransaction => true;

			public SqlServerStorageOptions() : base(null)
			{
			}
		}

		public class MySqlStorageOptions : SpiderOptions
		{
			public override string StorageConnectionString => "ConnectionString";
			public override string Storage => "DotnetSpider.DataFlow.Storage.MySqlEntityStorage,DotnetSpider.MySql";
			public override StorageType StorageType => StorageType.InsertAndUpdate;

			public override bool StorageIgnoreCase => false;
			public override int StorageRetryTimes => 800;
			public override bool StorageUseTransaction => true;

			public MySqlStorageOptions() : base(null)
			{
			}
		}

		public class MySqlFileStorageOptions : SpiderOptions
		{
			public override string StorageConnectionString => "ConnectionString";
			public override string Storage => "DotnetSpider.DataFlow.Storage.MySqlFileEntityStorage,DotnetSpider.MySql";
			public override StorageType StorageType => StorageType.InsertAndUpdate;

			public override bool StorageIgnoreCase => false;
			public override int StorageRetryTimes => 800;
			public override bool StorageUseTransaction => true;

			public override string MySqlFileType => "LoadFile";

			public MySqlFileStorageOptions() : base(null)
			{
			}
		}

		public class PostgreSqlStorageOptions : SpiderOptions
		{
			public override string StorageConnectionString => "ConnectionString";

			public override string Storage =>
				"DotnetSpider.DataFlow.Storage.PostgreSqlEntityStorage,DotnetSpider.PostgreSql";

			public override StorageType StorageType => StorageType.InsertAndUpdate;

			public override bool StorageIgnoreCase => false;
			public override int StorageRetryTimes => 800;
			public override bool StorageUseTransaction => true;

			public override string MySqlFileType => "LoadFile";

			public PostgreSqlStorageOptions() : base(null)
			{
			}
		}

		public class MongoStorageOptions : SpiderOptions
		{
			public override string StorageConnectionString => "mongodb://mongodb0.example.com:27017/admin";

			public override string Storage =>
				"DotnetSpider.DataFlow.Storage.MongoEntityStorage,DotnetSpider.Mongo";

			public override StorageType StorageType => StorageType.InsertAndUpdate;

			public override bool StorageIgnoreCase => false;
			public override int StorageRetryTimes => 800;
			public override bool StorageUseTransaction => true;

			public override string MySqlFileType => "LoadFile";

			public MongoStorageOptions() : base(null)
			{
			}
		}

		[Fact(DisplayName = "CreateDefaultStorage")]
		public void CreateDefaultStorage()
		{
			SpiderOptions options = new SqlServerStorageOptions();
			;
			// SqlServer
			var storage1 = (SqlServerEntityStorage)Spider.GetDefaultStorage(options);
			Assert.Equal("ConnectionString", storage1.ConnectionString);
			Assert.Equal(800, storage1.RetryTimes);
			Assert.True(storage1.UseTransaction);
			Assert.False(storage1.IgnoreCase);
			Assert.Equal(StorageType.InsertAndUpdate, storage1.StorageType);

			// MySql
			options = new MySqlStorageOptions();

			var storage2 = (MySqlEntityStorage)Spider.GetDefaultStorage(options);
			Assert.Equal("ConnectionString", storage2.ConnectionString);
			Assert.Equal(800, storage2.RetryTimes);
			Assert.True(storage2.UseTransaction);
			Assert.False(storage2.IgnoreCase);
			Assert.Equal(StorageType.InsertAndUpdate, storage2.StorageType);

			// MySqlFile
			options = new MySqlFileStorageOptions();

			var storage3 = (MySqlFileEntityStorage)Spider.GetDefaultStorage(options);
			Assert.False(storage3.IgnoreCase);
			Assert.Equal(MySqlFileType.LoadFile, storage3.MySqlFileType);

			// PostgreSql
			options = new PostgreSqlStorageOptions();

			var storage4 = (PostgreSqlEntityStorage)Spider.GetDefaultStorage(options);
			Assert.Equal("ConnectionString", storage4.ConnectionString);
			Assert.Equal(800, storage4.RetryTimes);
			Assert.True(storage4.UseTransaction);
			Assert.False(storage4.IgnoreCase);
			Assert.Equal(StorageType.InsertAndUpdate, storage4.StorageType);

			// Mongo
			options = new MongoStorageOptions();
			;

			var storage5 = (MongoEntityStorage)Spider.GetDefaultStorage(options);
			Assert.Equal("mongodb://mongodb0.example.com:27017/admin", storage5.ConnectionString);
		}

		[Fact(DisplayName = "RunThenExit")]
		public void RunThenExit()
		{
			var url = "http://www.RunThenExit.com/";
			var spider = LocalSpiderProvider.Value.Create<Spider>();

			spider.NewGuidId();
			spider.Name = "RunAsyncAndStop";
			for (var i = 0; i < 10000; i++)
			{
				spider.AddRequests(new Request(url + i) {DownloaderType = DownloaderType.Empty});
			}

			spider.RunAsync();
			Thread.Sleep(2000);
			spider.Pause();
			Thread.Sleep(2000);
			spider.Exit().WaitForExit();

			Assert.Equal(Status.Exited, spider.Status);
		}

		[Fact(DisplayName = "RunThenPauseThenContinueThenExit")]
		public void RunThenPauseThenContinueThenExit()
		{
			var url = "http://www.RunThenPauseThenContinueThenExit.com/";
			var spider = LocalSpiderProvider.Value.Create<Spider>();

			spider.NewGuidId();
			spider.Name = "RunAsyncAndStop";
			spider.EmptySleepTime = 15;

			for (var i = 0; i < 10000; i++)
			{
				spider.AddRequests(new Request(url + i) {DownloaderType = DownloaderType.Empty});
			}

			spider.RunAsync();
			Thread.Sleep(2000);
			spider.Pause();
			Thread.Sleep(2000);
			spider.Continue();
			Thread.Sleep(2000);
			spider.Exit().WaitForExit();

			Assert.Equal(Status.Exited, spider.Status);
		}

		/// <summary>
		/// 测试 MMF 关闭信号是否能正常工作
		/// </summary>
		[Fact(DisplayName = "MmfCloseSignal")]
		public void MmfCloseSignal()
		{
			var url = "http://www.MmfCloseSignal.com/";

			var spider = LocalSpiderProvider.Value.Create<Spider>();
			spider.MmfSignal = true;
			spider.NewGuidId();
			spider.Name = "MmfCloseSignal";

			for (var i = 0; i < 10000; i++)
			{
				spider.AddRequests(new Request(url + i) {DownloaderType = DownloaderType.Empty});
			}

			spider.RunAsync();
			Thread.Sleep(2000);
			spider.ExitBySignal().WaitForExit(15000);

			Assert.Equal(Status.Exited, spider.Status);
		}

		/// <summary>
		/// 1. 下载如果不正确是否有正常重试
		/// 2. 并且重试次数是否生效
		/// 3. 重试的请求的 Depth 不变
		/// </summary>
		[Fact(DisplayName = "RetryDownloadTimes")]
		public async Task RetryDownloadTimes()
		{
			var spider = LocalSpiderProvider.Value.Create<Spider>();
			spider.NewGuidId();
			spider.Name = "RetryDownloadTimes";
			spider.EmptySleepTime = 15;
			var scheduler = new QueueDistinctBfsScheduler();
			spider.Scheduler = scheduler;
			spider.AddRequests(new Request("http://www.RetryDownloadTimes.com")
			{
				DownloaderType = DownloaderType.Exception, RetryTimes = 5
			});
			await spider.RunAsync();

			var statisticsStore = LocalSpiderProvider.Value.GetRequiredService<IStatisticsStore>();
			var s = statisticsStore.GetSpiderStatisticsAsync(spider.Id).Result;
			Assert.Equal(1, s.Total);
			Assert.Equal(1, s.Failed);
			Assert.Equal(0, s.Success);

			var dss = statisticsStore.GetDownloadStatisticsListAsync(1, 10).Result;
			var ds = dss[0];
			Assert.Equal(6, ds.Failed);
			Assert.Equal(0, ds.Success);
		}

		[Fact(DisplayName = "DoNotRetryWhenResultIsEmpty")]
		public async Task DoNotRetryWhenResultIsEmpty()
		{
			var builder = new SpiderHostBuilder()
				.ConfigureLogging(x => x.AddSerilog())
				.ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
				.ConfigureServices(services =>
				{
					services.AddThroughMessageQueue();
					services.AddLocalDownloadCenter();
					services.AddDownloaderAgent(x =>
					{
						x.UseFileLocker();
						x.UseDefaultAdslRedialer();
						x.UseDefaultInternetDetector();
					});
					services.AddStatisticsCenter(x => x.UseMemory());
				});
			var provider = builder.Build();
			var spider = provider.Create<Spider>();
			spider.NewGuidId();
			spider.Name = "RetryWhenResultIsEmpty";
			spider.EmptySleepTime = 15;
			spider.RetryWhenResultIsEmpty = false;
			spider.Scheduler = new QueueDistinctBfsScheduler();
			spider.AddRequests(new Request("http://www.DoNotRetryWhenResultIsEmpty.com")
			{
				DownloaderType = DownloaderType.Empty, RetryTimes = 5
			});
			await spider.RunAsync();

			var statisticsStore = provider.GetRequiredService<IStatisticsStore>();
			var s = statisticsStore.GetSpiderStatisticsAsync(spider.Id).Result;

			var ds = statisticsStore.GetDownloadStatisticsListAsync(1, 10).Result[0];

			Assert.Equal(1, s.Total);
			Assert.Equal(0, s.Failed);
			Assert.Equal(1, s.Success);

			Assert.Equal(0, ds.Failed);
			Assert.Equal(1, ds.Success);
		}

		/// <summary>
		/// 1. 当所有 DataFlow 走完的时候，如果没有任何结析结果，RetryWhenResultIsEmpty 为 True 时会把当前 Request 添加回队列再次重试
		/// http://www.devfans.com/home/testempty 为一个可请求但是返回内容为空的测试地址
		/// 2. 重试的请求的 Depth 不变
		/// </summary>
		[Fact(DisplayName = "RetryWhenResultIsEmpty")]
		public void RetryWhenResultIsEmpty()
		{
			var builder = new SpiderHostBuilder()
				.ConfigureLogging(x => x.AddSerilog())
				.ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
				.ConfigureServices(services =>
				{
					services.AddThroughMessageQueue();
					services.AddLocalDownloadCenter();
					services.AddDownloaderAgent(x =>
					{
						x.UseFileLocker();
						x.UseDefaultAdslRedialer();
						x.UseDefaultInternetDetector();
					});
					services.AddStatisticsCenter(x => x.UseMemory());
				});

			var provider = builder.Build();
			var spider = provider.Create<Spider>();
			spider.NewGuidId();
			spider.Name = "RetryWhenResultIsEmpty";
			spider.EmptySleepTime = 15;
			spider.RetryWhenResultIsEmpty = true;
			spider.Scheduler = new QueueDistinctBfsScheduler();
			spider.AddRequests(new Request("http://www.RetryWhenResultIsEmpty.com")
			{
				DownloaderType = DownloaderType.Empty, RetryTimes = 5
			});
			spider.RunAsync().Wait();

			var statisticsStore = provider.GetRequiredService<IStatisticsStore>();
			var s = statisticsStore.GetSpiderStatisticsAsync(spider.Id).Result;

			var dss = statisticsStore.GetDownloadStatisticsListAsync(1, 10).Result;
			while (dss.Count == 0)
			{
				Thread.Sleep(1000);
			}

			var ds = dss[0];

			Assert.Equal(1, s.Total);
			Assert.Equal(1, s.Failed);
			Assert.Equal(0, s.Success);

			Assert.Equal(0, ds.Failed);
			Assert.Equal(6, ds.Success);
		}

		/// <summary>
		/// 检测 Spider._speedControllerInterval 的值是否设置正确
		/// 当 Spider.Speed 设置的值 n 大于 1 时，表示每秒下载 n 个链接，因此 speed interval 设置为 1 秒， 每秒从 scheduler 中取出 n 个链接，分发到各下载器去下载。
		/// 当 Spider.Speed 设置的值 n 大于 0 小于 1 时， 表示每秒下载个数要小于 1，因此不能用 1 秒做间隔， 而应该用 1/n
		/// Spider.Speed 的值必须大于 1
		/// </summary>
		[Fact(DisplayName = "SpeedInterval")]
		public void SpeedInterval()
		{
			//TODO:
		}

		/// <summary>
		/// 1. 设置 Depth 为 2，使用全站采集，检测目标链接深度大于 2 的是否为入队
		/// 2. 添加 Request 时，如果 Request 的 Depth <=0, 则修改 Depth 为 1， 其它其它使用用户设定的值
		/// </summary>
		[Fact(DisplayName = "Depth")]
		public void Depth()
		{
			//TODO:
		}

		/// <summary>
		/// 检测爬虫状态是否正确: running, paused, exiting, exited
		/// </summary>
		[Fact(DisplayName = "Status")]
		public void Status2()
		{
			//TODO:
		}

		/// <summary>
		/// 设置调度器，当爬虫的调度器已经有请求时不再允许更换
		/// </summary>
		[Fact(DisplayName = "Scheduler")]
		public void Scheduler()
		{
		}
	}
}
