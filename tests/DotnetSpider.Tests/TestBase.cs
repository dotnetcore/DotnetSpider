using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace DotnetSpider.Tests
{
	public class TestBase
	{
		protected readonly SpiderProvider SpiderFactory;

		protected TestBase()
		{
			var builder = new SpiderBuilder();
			builder.AddSerilog();
			builder.ConfigureAppConfiguration(null, null, false);
			builder.UseStandalone();
			SpiderFactory = builder.Build();

			SpiderFactory.GetRequiredService<ILogger<TestBase>>()
				.LogInformation($"Development {SpiderFactory.GetRequiredService<IConfiguration>()["Development"]}");
		}
	}
}