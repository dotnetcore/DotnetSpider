using System;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Log;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class Scheduler
	{
		[Flags]
		public enum Types
		{
			Queue,
			Redis
		}

		public abstract Types Type { get; internal set; }
		public abstract IScheduler GetScheduler();
	}

	public class RedisScheduler : Scheduler
	{
		public string Host { get; set; }
		public int Port { get; set; } = 6379;
		public string Password { get; set; }

		public override Types Type { get; internal set; } = Types.Redis;

		public override IScheduler GetScheduler()
		{
			return new Extension.Scheduler.RedisScheduler(Host, Password, Port);
		}
	}

	public class QueueScheduler : Scheduler
	{
		public override Types Type { get; internal set; } = Types.Queue;

		public override IScheduler GetScheduler()
		{
			return new QueueDuplicateRemovedScheduler();
		}
	}
}
