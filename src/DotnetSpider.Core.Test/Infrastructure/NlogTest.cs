using DotnetSpider.Core.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Test.Infrastructure
{
	[TestClass]
	public class NlogTest
	{
		[TestMethod]
		public void WithoutNlogConfig()
		{
#if !NET_CORE
			string nlogConfigPath = Path.Combine(Core.Infrastructure.Environment.BaseDirectory, "nlog.net45.config");
#else
			string nlogConfigPath = Path.Combine(Core.Infrastructure.Environment.BaseDirectory, "nlog.config");
#endif

#if !NET_CORE
			var tmpPath = Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"), "nlog.net45.config");
#else
			var tmpPath = Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"), "nlog.config");
#endif
			try
			{
				File.Move(nlogConfigPath, tmpPath);

				ILogger logger = LogCenter.GetLogger();

				using (StreamReader reader = new StreamReader(typeof(LogCenter).Assembly.GetManifestResourceStream("DotnetSpider.Core.nlog.default.config")))
				{
					var nlogConfig = reader.ReadToEnd();
					Assert.AreEqual(nlogConfig, File.ReadAllText(nlogConfigPath));
				}
			}
			finally
			{
				File.Delete(nlogConfigPath);
				File.Move(tmpPath, nlogConfigPath);
			}
		}
	}
}
