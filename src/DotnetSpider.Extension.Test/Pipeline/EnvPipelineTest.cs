using DotnetSpider.Core;
using System.IO;
using Xunit;

namespace DotnetSpider.Extension.Test.Pipeline
{
	public class EnvPipelineTest
	{
		private void PrepareGlobalFile(string name)
		{
			var path = Path.Combine(Env.GlobalDirectory, name);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.Copy(name, path);
		}

		[Fact(DisplayName = "EnvConfigSetEmpty")]
		public void EnvConfigSetEmpty()
		{
			lock (Env.BaseDirectory)
			{
				var arguments1 = Startup.Parse("-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "--tid:TestSpider", "-i:guid", "-a:", "-c:");
				Startup.LoadConfiguration(arguments1.Config);

				Assert.Equal("app.config", Env.EmailAccount);
			}
		}


		[Fact(DisplayName = "EnvSetGlobal1")]
		public void EnvSetGlobal1()
		{
			lock (Env.BaseDirectory)
			{
				PrepareGlobalFile("app.global.1.config");
				var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "--tid:TestSpider", "-i:guid", "-a:", "-c:%GLOBAL%app.global.1.config" };
				var arguments1 = Startup.Parse(args1);
				Startup.LoadConfiguration(arguments1.Config);
				Assert.Equal("app.global.1.config", Env.EmailAccount);
			}
		}

		[Fact(DisplayName = "EnvSetGlobal2")]
		public void EnvSetGlobal2()
		{
			lock (Env.BaseDirectory)
			{
				PrepareGlobalFile("app.config");
				var args1 = new[] { "-s:DotnetSpider.Extension.Test.Pipeline.TestSpider2", "--tid:TestSpider", "-i:guid", "-a:", "-c:%GLOBAL%" };
				var arguments1 = Startup.Parse(args1);
				Startup.LoadConfiguration(arguments1.Config);
				Assert.Equal("app.config", Env.EmailAccount);
			}
		}
	}
}
