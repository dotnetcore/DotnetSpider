using System.IO;
using Xunit;

namespace DotnetSpider.Core.Test
{
	public class EnvironmentTest
	{
		[Fact(DisplayName = "DefaultConfig")]
		public void DefaultConfig()
		{
			if (File.Exists(Env.DefaultGlobalAppConfigPath))
			{
				File.Delete(Env.DefaultGlobalAppConfigPath);
			}
			File.Copy("app.global.config", Env.DefaultGlobalAppConfigPath);
			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-c:%GLOBAL%app.config" };
			var arguments1 = Startup.Parse(args1);
			Startup.LoadConfiguration(arguments1.Config);
			Assert.Equal("Database='mysql';Data Source=192.168.90.100;User ID=user20170913;Password=KenTYDrZJOeUEvlP3NE&$pouzrk6gXD#;Port=53306;SslMode=None", Env.DataConnectionString);
			Env.LoadConfiguration();
			Assert.Equal("Database='mysql';Data Source=localhost;User ID=root;Port=3306;SslMode=None;", Env.DataConnectionString);
		}
	}
}
