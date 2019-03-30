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
        }
    }
}