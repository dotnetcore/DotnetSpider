using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Options;

namespace DotnetSpider.MySql.Scheduler
{
	public class MySqlQueueBfsScheduler : MySqlQueueScheduler
	{
		public MySqlQueueBfsScheduler(IRequestHasher requestHasher,
			IOptions<MySqlSchedulerOptions> options) : base(requestHasher, options)
		{
		}

		protected override string DequeueSql =>
			"SELECT id, hash, request FROM {0}_queue ORDER BY id ASC LIMIT {1} FOR UPDATE;";
	}
}
