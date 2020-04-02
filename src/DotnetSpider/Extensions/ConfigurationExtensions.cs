using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Extensions
{
    public static class ConfigurationExtensions
    {
        public static bool IsDistributed(this IConfiguration configuration)
        {
            return configuration["DOTNET_SPIDER_MODEL"] == "DISTRIBUTED";
        }
    }
}