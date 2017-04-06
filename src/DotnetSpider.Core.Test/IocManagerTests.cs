//using System;
//using System.Linq;
//using DotnetSpider.Core.Monitor;
//

//namespace DotnetSpider.Core.Test
//{
//	public class TestMonitor : IMonitor
//	{
//		public void Dispose()
//		{
//			throw new NotImplementedException();
//		}

//		public void Report(SpiderStatus spider)
//		{
//			throw new NotImplementedException();
//		}

//		public bool IsEnabled { get; }
//	}

//	public class TestMonitor2 : IMonitor
//	{
//		public void Dispose()
//		{
//			throw new NotImplementedException();
//		}

//		public void Report(SpiderStatus spider)
//		{
//			throw new NotImplementedException();
//		}

//		public bool IsEnabled { get; }
//	}

//	public class IocManagerTests
//	{
//		[TestMethod]
//		public void Ioc1()
//		{
//			IocManager.AddSingleton<IMonitor, NLogMonitor>();
//			var nlogMonitor = IocManager.GetService<IMonitor>();
//			Assert.NotNull(nlogMonitor);

//			IocManager.AddSingleton<IMonitor, TestMonitor>();
//			var monitor = IocManager.GetService<IMonitor>();

//			Assert.AreEqual(typeof(TestMonitor).FullName, monitor.GetType().FullName);
//		}

//		[TestMethod]
//		public void Ioc2()
//		{
//			IocManager.AddTransient<IMonitor, NLogMonitor>();
//			var nlogMonitor = IocManager.GetService<IMonitor>();
//			Assert.NotNull(nlogMonitor);

//			IocManager.AddTransient<IMonitor, TestMonitor>();
//			var monitor = IocManager.GetService<IMonitor>();

//			Assert.AreEqual(typeof(TestMonitor).FullName, monitor.GetType().FullName);
//		}

//		[TestMethod]
//		public void Ioc3()
//		{
//			IocManager.AddTransient<NLogMonitor, NLogMonitor>();
//			var nlogMonitor = IocManager.GetService<NLogMonitor>();
//			Assert.NotNull(nlogMonitor);

//			IocManager.AddTransient<TestMonitor, TestMonitor>();
//			var monitor = IocManager.GetService<TestMonitor>();

//			Assert.AreEqual(typeof(TestMonitor).FullName, monitor.GetType().FullName);
//		}

//		[TestMethod]
//		public void Ioc4()
//		{
//			IocManager.AddTransient<IMonitor, NLogMonitor>();


//			IocManager.AddTransient<IMonitor, TestMonitor>();
//			var monitors = IocManager.GetServices<IMonitor>().ToList();

//			Assert.AreEqual(2, monitors.Count);
//			Assert.AreEqual(typeof(NLogMonitor).FullName, monitors[0].GetType().FullName);
//			Assert.AreEqual(typeof(TestMonitor).FullName, monitors[1].GetType().FullName);
//		}
//	}
//}
