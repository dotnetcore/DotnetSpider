using StackExchange.Redis;

namespace DotnetSpider.Extension.Infrastructure
{
    public class RedisConnection
    {
        public string ConnectString { get; }
        public IDatabase Database { get; }
        public ISubscriber Subscriber { get; }

        public RedisConnection(string connectString)
        {
            ConnectString = connectString;

            var connection = ConnectionMultiplexer.Connect(connectString);
            Database = connection.GetDatabase(0);
            Subscriber = connection.GetSubscriber();
        }
    }
}