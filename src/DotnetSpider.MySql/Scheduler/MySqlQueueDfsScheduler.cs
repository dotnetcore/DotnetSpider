using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Options;

namespace DotnetSpider.MySql.Scheduler;

public class MySqlQueueDfsScheduler(IRequestHasher requestHasher, IOptions<MySqlSchedulerOptions> options)
    : MySqlQueueScheduler(requestHasher, options)
{
    protected override string DequeueSql =>
        "SELECT id, hash, request FROM {0}_queue ORDER BY id DESC LIMIT {1} FOR UPDATE;";
}