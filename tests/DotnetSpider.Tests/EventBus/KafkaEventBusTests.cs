using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Kafka;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DotnetSpider.Tests.MessageQueue
{
	public class KafkaEventBusTests : TestBase
	{
		[Fact(DisplayName = "PubAndSub")]
		public async Task PubAndSub()
		{
			if (IsCI())
			{
				return;
			}

			var count = 0;
			var options = DistributeSpiderProvider.Value.GetRequiredService<KafkaOptions>();
			var logger = DistributeSpiderProvider.Value.GetRequiredService<ILogger<KafkaMq>>();
			var mq = new KafkaMq(options, logger);
			mq.Subscribe<string>("PubAndSub", msg => { Interlocked.Increment(ref count); });
			for (var i = 0; i < 100; ++i)
			{
				await mq.PublishAsync("PubAndSub", new MessageData<string>() {Data = "a"});
			}

			var j = 0;
			while (count < 100 && j < 150)
			{
				Thread.Sleep(500);
				++j;
			}

			Assert.Equal(100, count);
		}

		[Fact(DisplayName = "ParallelPubAndSub")]
		public void ParallelPubAndSub()
		{
			if (IsCI())
			{
				return;
			}

			var count = 0;
			var options = DistributeSpiderProvider.Value.GetRequiredService<KafkaOptions>();
			var logger = DistributeSpiderProvider.Value.GetRequiredService<ILogger<KafkaMq>>();
			var mq = new KafkaMq(options, logger);
			mq.Subscribe<string>("ParallelPubAndSub", msg => { Interlocked.Increment(ref count); });

			Parallel.For(0, 100, async i =>
			{
				await mq.PublishAsync("ParallelPubAndSub", new MessageData<string>() {Data = "a"});
			});
			var j = 0;
			while (count < 100 && j < 150)
			{
				Thread.Sleep(500);
				++j;
			}

			Assert.Equal(100, count);
		}

		[Fact(DisplayName = "PubAndUnSub")]
		public async Task PubAndUnSub()
		{
			if (IsCI())
			{
				return;
			}

			var count = 0;
			var options = DistributeSpiderProvider.Value.GetRequiredService<KafkaOptions>();
			var logger = DistributeSpiderProvider.Value.GetRequiredService<ILogger<KafkaMq>>();
			var mq = new KafkaMq(options, logger);
			mq.Subscribe<string>("PubAndUnSub", msg => { Interlocked.Increment(ref count); });

			var i = 0;
			Task.Factory.StartNew(async () =>
			{
				for (; i < 50; ++i)
				{
					await mq.PublishAsync("PubAndUnSub", new MessageData<string>() {Data = "a"});
					await Task.Delay(100);
				}
			}).ConfigureAwait(false).GetAwaiter();
			await Task.Delay(1500);
			mq.Unsubscribe("PubAndUnSub");

			while (i < 50)
			{
				await Task.Delay(100);
			}

			Assert.True(count < 100);
		}
	}
}
