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
			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-e:CONFIG=app.outside.config,DBCONFIG=GLOBAL" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.SetEnviroment(arguments1);

			Core.Environment.Reload();
			Assert.Equal("GLOBAL", AppDomain.CurrentDomain.GetData(Environment.EnvDbConfig)?.ToString());
			Assert.Equal("app.outside.config", AppDomain.CurrentDomain.GetData(Environment.EnvConfig)?.ToString());

			Assert.Equal("Database='mysql';Data Source=OUTSITE;User ID=root;Port=3306;SslMode=None;", Environment.DataConnectionString);

			AppDomain.CurrentDomain.SetData("CONFIG", "");
			AppDomain.CurrentDomain.SetData("DBCONFIG", "");
		}

		[Fact]
		public void DefaultConfig()
		{
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
			var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "-tid:TestSpider", "-i:guid", "-a:", "-e:CONFIG=app.inside.config,DBCONFIG=GLOBAL" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Startup.SetEnviroment(arguments1);
			Core.Environment.Reload();
			Assert.Equal("GLOBAL", AppDomain.CurrentDomain.GetData(Environment.EnvDbConfig)?.ToString());
			Assert.Equal("app.inside.config", AppDomain.CurrentDomain.GetData(Environment.EnvConfig)?.ToString());

			Assert.Equal("Database='mysql';Data Source=INSIDE;User ID=root;Port=3306;SslMode=None;", Environment.DataConnectionString);

			AppDomain.CurrentDomain.SetData("CONFIG", "");
			AppDomain.CurrentDomain.SetData("DBCONFIG", "");
		}
	}
}
