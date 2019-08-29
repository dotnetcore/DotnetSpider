using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.MessageQueue;
using Xunit;

namespace DotnetSpider.Tests.MessageQueue
{
	public class LocalEventBusTests : TestBase
	{
		[Fact(DisplayName = "PubAndSub")]
		public async Task PubAndSub()
		{
			var count = 0;
			var mq = new ThroughMessageQueue(CreateLogger<ThroughMessageQueue>());
			mq.Subscribe<string>("topic", msg => { Interlocked.Increment(ref count); });
			for (var i = 0; i < 100; ++i)
			{
				await mq.PublishAsync("topic", new MessageData<string>
				{
					Data = "a"
				});
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
			var count = 0;
			var mq = new ThroughMessageQueue(CreateLogger<ThroughMessageQueue>());
			mq.Subscribe<string>("topic", msg => { Interlocked.Increment(ref count); });

			Parallel.For(0, 100, async i =>
			{
				await mq.PublishAsync("topic", new MessageData<string>
				{
					Data = "a"
				});
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
			var count = 0;
			var mq = new ThroughMessageQueue(CreateLogger<ThroughMessageQueue>());
			mq.Subscribe<string>("topic", msg => { Interlocked.Increment(ref count); });

			var i = 0;
			Task.Factory.StartNew(async () =>
			{
				for (; i < 50; ++i)
				{
					await mq.PublishAsync("topic", new MessageData<string>
					{
						Data = "a"
					});
					await Task.Delay(100);
				}
			}).ConfigureAwait(false).GetAwaiter();
			await Task.Delay(1500);
			mq.Unsubscribe("topic");

			while (i < 50)
			{
				await Task.Delay(100);
			}

			Assert.True(count < 100);
		}
	}
}
