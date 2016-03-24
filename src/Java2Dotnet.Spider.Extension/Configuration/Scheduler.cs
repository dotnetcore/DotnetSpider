using System;
using Java2Dotnet.Spider.Core.Scheduler;
#if NET_CORE
using Java2Dotnet.Spider.JLog;
#endif

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class Scheduler : IJobject
	{
#if NET_CORE
		protected static ILog Logger = LogManager.GetLogger();
#endif
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
		public int Port { get; set; }
		public string Password { get; set; }

		public override Types Type { get; internal set; } = Types.Redis;

		public override IScheduler GetScheduler()
		{
#if NET_CORE
			Logger.Info($"Redis Info: {Host} {Port} {Password}",true);
#endif
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
