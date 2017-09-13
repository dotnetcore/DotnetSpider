using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace DotnetSpider.Core.Test
{
	public class EnvironmentTest
	{
		[Fact]
		public void OutsideConfig()
		{
			if (File.Exists(Core.Environment.GlobalAppConfigPath))
			{
				File.Delete(Core.Environment.GlobalAppConfigPath);
			}
			File.Copy("app.global.config", Core.Environment.GlobalAppConfigPath);

			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-e:CONFIG=app.outside.config,DBCONFIG=GLOBAL" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.SetEnviroment(arguments1);

			Core.Environment.Reload();
			Assert.Equal("GLOBAL", AppDomain.CurrentDomain.GetData(Environment.EnvDbConfig)?.ToString());
			Assert.Equal("app.outside.config", AppDomain.CurrentDomain.GetData(Environment.EnvConfig)?.ToString());

			Assert.Equal("Database='mysql';Data Source=192.168.90.100;User ID=root;Port=3306;SslMode=None;", Environment.DataConnectionString);
			Assert.Equal("OUTSITE:6379,serviceName=DotnetSpider,keepAlive=8,allowAdmin=True,connectTimeout=10000,abortConnect=True,connectRetry=20", Environment.RedisConnectString);

			AppDomain.CurrentDomain.SetData("CONFIG", "");
			AppDomain.CurrentDomain.SetData("DBCONFIG", "");
		}

		[Fact]
		public void DefaultConfig()
		{
			if (File.Exists(Core.Environment.GlobalAppConfigPath))
			{
				File.Delete(Core.Environment.GlobalAppConfigPath);
			}
			File.Copy("app.global.config", Core.Environment.GlobalAppConfigPath);
			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-e:" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.SetEnviroment(arguments1);
			Core.Environment.Reload();
			Assert.Null(AppDomain.CurrentDomain.GetData(Environment.EnvDbConfig)?.ToString());
			Assert.Null(AppDomain.CurrentDomain.GetData(Environment.EnvConfig)?.ToString());

			Assert.Equal("Database='mysql';Data Source=localhost;User ID=root;Port=3306;SslMode=None;", Environment.DataConnectionString);

			AppDomain.CurrentDomain.SetData("CONFIG", "");
			AppDomain.CurrentDomain.SetData("DBCONFIG", "");
		}

		[Fact]
		public void InsideConfig()
		{
			if (File.Exists(Core.Environment.GlobalAppConfigPath))
			{
				File.Delete(Core.Environment.GlobalAppConfigPath);
			}
			File.Copy("app.global.config", Core.Environment.GlobalAppConfigPath);
			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-e:CONFIG=app.inside.config,DBCONFIG=GLOBAL" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.SetEnviroment(arguments1);
			Core.Environment.Reload();
			Assert.Equal("GLOBAL", AppDomain.CurrentDomain.GetData(Environment.EnvDbConfig)?.ToString());
			Assert.Equal("app.inside.config", AppDomain.CurrentDomain.GetData(Environment.EnvConfig)?.ToString());

			Assert.Equal("Database='mysql';Data Source=192.168.90.100;User ID=root;Port=3306;SslMode=None;", Environment.DataConnectionString);
			Assert.Equal("INSITE:6379,serviceName=DotnetSpider,keepAlive=8,allowAdmin=True,connectTimeout=10000,abortConnect=True,connectRetry=20", Environment.RedisConnectString);

			AppDomain.CurrentDomain.SetData("CONFIG", "");
			AppDomain.CurrentDomain.SetData("DBCONFIG", "");
		}
	}
}
