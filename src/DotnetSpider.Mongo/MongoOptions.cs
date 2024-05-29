using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Mongo;

public class MongoOptions(IConfiguration configuration)
{
    public string ConnectionString => configuration["Mongo:ConnectionString"];
}