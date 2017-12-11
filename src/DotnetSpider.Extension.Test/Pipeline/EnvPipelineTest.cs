using DotnetSpider.Core;
using DotnetSpider.Extension.Pipeline;
using System;
using System.IO;
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
			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-c:" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.LoadConfiguration(arguments1);
		}

		[Fact]
		public void EnvSet()
		{
			if (File.Exists(Env.DefaultGlobalAppConfigPath))
			{
				File.Delete(Env.DefaultGlobalAppConfigPath);
			}
			File.Copy("app.global.config", Env.DefaultGlobalAppConfigPath);
			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-c:%GLOBAL%" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.LoadConfiguration(arguments1);
		}

		[Fact]
		public void EnvSetGloablAppMissingPipeline()
		{
			if (File.Exists(Env.DefaultGlobalAppConfigPath))
			{
				File.Delete(Env.DefaultGlobalAppConfigPath);
			}
			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-c:%GLOBAL%" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.LoadConfiguration(arguments1);

			Env.Reload();
		}

		[Fact]
		public void EnvSetPipeline()
		{
			if (File.Exists(Env.DefaultGlobalAppConfigPath))
			{
				File.Delete(Env.DefaultGlobalAppConfigPath);
			}
			File.Copy(Path.Combine(AppContext.BaseDirectory, "app.global.config"), Env.DefaultGlobalAppConfigPath);

			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-c:%GLOBAL%" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.LoadConfiguration(arguments1);

			MySqlEntityPipeline pipeline = new MySqlEntityPipeline();
			var a = pipeline.ConnectionStringSettings;
			Assert.Equal("Database='mysql';Data Source=192.168.90.101;User ID=user20170913;Password=KenTYDrZJOeUEvlP3NE&$pouzrk6gXD#;Port=53306;SslMode=None", a.ConnectionString);
			Assert.Equal("127.0.0.101:6379,serviceName=DotnetSpider,keepAlive=8,allowAdmin=True,connectTimeout=10000,abortConnect=True,connectRetry=20", Env.RedisConnectString);

			Env.Reload();
		}

		[Fact]
		public void EnvUnSetPipeline()
		{
			if (File.Exists(Env.DefaultGlobalAppConfigPath))
			{
				File.Delete(Env.DefaultGlobalAppConfigPath);
			}
			File.Copy("app.global.config", Env.DefaultGlobalAppConfigPath);

			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.LoadConfiguration(arguments1);

			Env.Reload();

			MySqlEntityPipeline pipeline = new MySqlEntityPipeline();
			var a = pipeline.ConnectionStringSettings;
			Assert.Equal("Database='mysql';Data Source=localhost;User ID=root;Port=3306;SslMode=None;", a.ConnectionString);
		}
	}
}
