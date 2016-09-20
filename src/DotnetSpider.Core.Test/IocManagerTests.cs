using System;
using DotnetSpider.Core.Monitor;
using Xunit;

namespace DotnetSpider.Core.Test
{
	public class TestMonitor : IMonitorService
	{
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public void Watch(SpiderStatus spider)
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled { get; }
	}

	public class TestMonitor2 : IMonitorService
	{
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public void Watch(SpiderStatus spider)
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
			IocContainer.Default.AddSingleton<IMonitorService, NLogMonitor>();
			var nlogMonitor = IocContainer.Default.GetService<IMonitorService>();
			Assert.NotNull(nlogMonitor);

			IocContainer.Default.AddSingleton<IMonitorService, TestMonitor>();
			var monitor = IocContainer.Default.GetService<IMonitorService>();

			Assert.Equal(typeof(NLogMonitor).FullName,monitor.GetType().FullName);
		}

		[Fact]
		public void Ioc2()
		{
			IocContainer.Default.AddTransient<IMonitorService, NLogMonitor>();
			var nlogMonitor = IocContainer.Default.GetService<IMonitorService>();
			Assert.NotNull(nlogMonitor);

			IocContainer.Default.AddTransient<IMonitorService, TestMonitor>();
			var monitor = IocContainer.Default.GetService<IMonitorService>();

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
	}
}
