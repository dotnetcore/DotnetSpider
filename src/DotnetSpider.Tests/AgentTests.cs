using System.Threading.Tasks;
using DotnetSpider.Agent;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace DotnetSpider.Tests;

public class AgentTests
{
    [Fact]
    public async Task Start()
    {
        var builder = new HostBuilder();
        builder.ConfigureServices(collection =>
        {
            collection.AddAgentHostService();
        });
        await builder.Build().RunAsync();
    }
}
