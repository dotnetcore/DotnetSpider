using System;
using System.Linq;
using DotnetSpider.Core.Monitor;
using Xunit;

namespace DotnetSpider.Core.Test
{
	public class TestMonitor : IMonitor
	{
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public void Report(SpiderStatus spider)
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled { get; }
	}

	public class TestMonitor2 : IMonitor
	{
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public void Report(SpiderStatus spider)
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled { get; }
	}

	public class IocManagerTests
	{
		[Fact]
		public void Ioc1()
		{
			IocContainer.Default.AddSingleton<IMonitor, NLogMonitor>();
			var nlogMonitor = IocContainer.Default.GetService<IMonitor>();
			Assert.NotNull(nlogMonitor);

			IocContainer.Default.AddSingleton<IMonitor, TestMonitor>();
			var monitor = IocContainer.Default.GetService<IMonitor>();

			Assert.Equal(typeof(TestMonitor).FullName, monitor.GetType().FullName);
		}

		[Fact]
		public void Ioc2()
		{
			IocContainer.Default.AddTransient<IMonitor, NLogMonitor>();
			var nlogMonitor = IocContainer.Default.GetService<IMonitor>();
			Assert.NotNull(nlogMonitor);

			IocContainer.Default.AddTransient<IMonitor, TestMonitor>();
			var monitor = IocContainer.Default.GetService<IMonitor>();

			Assert.Equal(typeof(TestMonitor).FullName, monitor.GetType().FullName);
		}

		[Fact]
		public void Ioc3()
		{
			IocContainer.Default.AddTransient<NLogMonitor, NLogMonitor>();
			var nlogMonitor = IocContainer.Default.GetService<NLogMonitor>();
			Assert.NotNull(nlogMonitor);

			IocContainer.Default.AddTransient<TestMonitor, TestMonitor>();
			var monitor = IocContainer.Default.GetService<TestMonitor>();

			Assert.Equal(typeof(TestMonitor).FullName, monitor.GetType().FullName);
		}

		[Fact]
		public void Ioc4()
		{
			IocContainer.Default.AddTransient<IMonitor, NLogMonitor>();


			IocContainer.Default.AddTransient<IMonitor, TestMonitor>();
			var monitors = IocContainer.Default.GetServices<IMonitor>().ToList();

			Assert.Equal(2, monitors.Count);
			Assert.Equal(typeof(NLogMonitor).FullName, monitors[0].GetType().FullName);
			Assert.Equal(typeof(TestMonitor).FullName, monitors[1].GetType().FullName);
		}
	}
}
