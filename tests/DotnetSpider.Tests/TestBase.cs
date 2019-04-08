using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;


namespace DotnetSpider.Tests
{
	public class TestBase
	{
		protected readonly Lazy<SpiderProvider> SpiderProvider = new Lazy<SpiderProvider>(() =>
		{
			var builder = new SpiderBuilder();
			builder.AddSerilog();
			builder.ConfigureAppConfiguration(null, null, false);
			builder.UseStandalone();
			return builder.Build();
		});

		protected bool IsCI()
		{
			return Directory.Exists("/home/vsts/work");
		}

		protected ILogger<T> CreateLogger<T>()
		{
			var logger = new LoggerFactory();
			logger.AddSerilog();
			return logger.CreateLogger<T>();
		}
	}
}