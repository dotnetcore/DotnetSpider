using System;
using System.IO;
using Xunit;

namespace DotnetSpider.Core.Test
{
	public class EnvironmentTest
	{

		[Fact]
		public void DefaultConfig()
		{
			if (File.Exists(Env.DefaultGlobalAppConfigPath))
			{
				File.Delete(Env.DefaultGlobalAppConfigPath);
			}
			File.Copy("app.global.config", Env.DefaultGlobalAppConfigPath);
			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-c:%GLOBAL%" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.LoadConfiguration(arguments1);
			Env.Reload();
			Assert.Equal("Database='mysql';Data Source=localhost;User ID=root;Port=3306;SslMode=None;", Env.DataConnectionString);
		}
	}
}
