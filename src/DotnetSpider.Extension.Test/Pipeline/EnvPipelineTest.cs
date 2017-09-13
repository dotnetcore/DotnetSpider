using DotnetSpider.Core;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace DotnetSpider.Extension.Test.Pipeline
{
	public class TestSpider2 : Spider
	{
		public TestSpider2()
		{
			Name = "hello";
		}
	}

	public class EnvPipelineTest
	{
		[Fact]
		public void EnvSetNull()
		{
			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-e:" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.SetEnviroment(arguments1);
		}

		[Fact]
		public void EnvSet()
		{
			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-e:DBCONFIG=GLOBAL" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.SetEnviroment(arguments1);

			Assert.Equal("GLOBAL", AppDomain.CurrentDomain.GetData(Core.Environment.EnvDbConfig)?.ToString());

			AppDomain.CurrentDomain.SetData("CONFIG", "");
			AppDomain.CurrentDomain.SetData("DBCONFIG", "");
		}

		[Fact]
		public void EnvSetGloablAppMissingPipeline()
		{
			try
			{
				if (File.Exists(Core.Environment.GlobalAppConfigPath))
				{
					File.Delete(Core.Environment.GlobalAppConfigPath);
				}
				var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-e:DBCONFIG=GLOBAL" };
				var arguments1 = Startup.AnalyzeArguments(args1);
				Startup.SetEnviroment(arguments1);

				Core.Environment.Reload();

				MySqlEntityPipeline pipeline = new MySqlEntityPipeline();

				Assert.Throws<SpiderException>(() =>
				{
					var a = pipeline.ConnectionStringSettings;
				});
			}
			finally
			{
				AppDomain.CurrentDomain.SetData("CONFIG", "");
				AppDomain.CurrentDomain.SetData("DBCONFIG", "");
			}

		}

		[Fact]
		public void EnvSetPipeline()
		{
			if (File.Exists(Core.Environment.GlobalAppConfigPath))
			{
				File.Delete(Core.Environment.GlobalAppConfigPath);
			}
			File.Copy("app.global.config", Core.Environment.GlobalAppConfigPath);

			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-e:DBCONFIG=GLOBAL" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.SetEnviroment(arguments1);

			Core.Environment.Reload();

			MySqlEntityPipeline pipeline = new MySqlEntityPipeline();
			var a = pipeline.ConnectionStringSettings;
			Assert.Equal("Database='mysql';Data Source=localhost2;User ID=root;Port=3306;SslMode=None;", a.ConnectionString);
			Assert.Equal("127.0.0.1:6379,serviceName=DotnetSpider,keepAlive=8,allowAdmin=True,connectTimeout=10000,abortConnect=True,connectRetry=20", Core.Environment.RedisConnectString);

			AppDomain.CurrentDomain.SetData("CONFIG", "");
			AppDomain.CurrentDomain.SetData("DBCONFIG", "");
		}

		[Fact]
		public void EnvUnSetPipeline()
		{

			if (File.Exists(Core.Environment.GlobalAppConfigPath))
			{
				File.Delete(Core.Environment.GlobalAppConfigPath);
			}
			File.Copy("app.global.config", Core.Environment.GlobalAppConfigPath);

			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.SetEnviroment(arguments1);

			Core.Environment.Reload();

			MySqlEntityPipeline pipeline = new MySqlEntityPipeline();
			var a = pipeline.ConnectionStringSettings;
			Assert.Equal("Database='mysql';Data Source=localhost;User ID=root;Port=3306;SslMode=None;", a.ConnectionString);

			AppDomain.CurrentDomain.SetData("CONFIG", "");
			AppDomain.CurrentDomain.SetData("DBCONFIG", "");
		}
	}
}
