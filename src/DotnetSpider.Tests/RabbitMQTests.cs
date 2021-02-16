using System;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using DotnetSpider.MessageQueue;
using DotnetSpider.RabbitMQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace DotnetSpider.Tests
{
	public class RabbitMQTests
	{
		private readonly ITestOutputHelper _testOutputHelper;

		public RabbitMQTests(ITestOutputHelper testOutputHelper)
		{
			_testOutputHelper = testOutputHelper;
		}

		public class MyOptions : IOptions<RabbitMQOptions>
		{
			public RabbitMQOptions Value => new()
			{
				Exchange = "Test", HostName = "localhost", UserName = "user", Password = "password"
			};
		}

		protected virtual IMessageQueue GetMessageQueue()
		{
			var mq = new RabbitMQMessageQueue(new MyOptions(), new LoggerFactory());
			return mq;
		}

		[Fact]
		public async Task Consumer()
		{
			var messageQueue = GetMessageQueue();
			var consumer = new AsyncMessageConsumer<byte[]>("test");
			consumer.Received += bytes => null;
			await messageQueue.ConsumeAsync(consumer, default);
			messageQueue.CloseQueue("test");
		}

		[Fact]
		public async Task Close()
		{
			var messageQueue = GetMessageQueue();
			var queue = Guid.NewGuid().ToString("N");
			var consumer = new AsyncMessageConsumer<byte[]>(queue);
			var counter = 0;
			consumer.Received += async bytes =>
			{
				var message = (Message)await bytes.DeserializeAsync(default);
				counter = message.Index;
			};
			await messageQueue.ConsumeAsync(consumer, default);
			await messageQueue.PublishAsBytesAsync(queue, new Message {Index = 1000});
			Sleep();
			Assert.Equal(1000, counter);
			counter = 0;
			consumer.Close();
			Sleep();
			await messageQueue.PublishAsBytesAsync(queue, new Message {Index = 2000});
			Sleep();
			Assert.Equal(0, counter);
			messageQueue.CloseQueue(queue);
		}


		[Fact]
		public void ConcurrentConsume()
		{
			var messageQueue = GetMessageQueue();
			Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 8}, async i =>
			{
				var consumer = new AsyncMessageConsumer<byte[]>("test");
				consumer.Received += bytes =>
				{
					_testOutputHelper.WriteLine("hi");
					return Task.CompletedTask;
				};
				await messageQueue.ConsumeAsync(consumer, default);
			});
			messageQueue.CloseQueue("test");
		}

		[Fact]
		public async Task Publish()
		{
			var messageQueue = GetMessageQueue();
			var queue = Guid.NewGuid().ToString("N");
			var consumer = new AsyncMessageConsumer<byte[]>(queue);
			var counter = 0;
			consumer.Received += async bytes =>
			{
				var message = (Message)await bytes.DeserializeAsync(default);
				counter = message.Index;
				await Task.CompletedTask;
			};
			await messageQueue.ConsumeAsync(consumer, default);
			await messageQueue.PublishAsBytesAsync(queue, new Message {Index = 1000});
			Sleep();
			Assert.Equal(1000, counter);
			messageQueue.CloseQueue(queue);
		}

		public class Message
		{
			public int Index { get; set; }
		}

		[Fact]
		public virtual async Task ConcurrentPublish()
		{
			var messageQueue = GetMessageQueue();
			var queue = Guid.NewGuid().ToString("N");
			var consumer = new AsyncMessageConsumer<byte[]>(queue);
			var counter = 0;
			consumer.Received += async bytes =>
			{
				lock (this)
				{
					counter += 1;
				}

				await Task.CompletedTask;
			};
			await messageQueue.ConsumeAsync(consumer, default);

			Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 8}, async (i) =>
			{
				await messageQueue.PublishAsBytesAsync(queue, new Message {Index = i});
			});

			Sleep();

			Assert.Equal(1000, counter);
			messageQueue.CloseQueue(queue);
		}


		protected virtual void Sleep()
		{
			Thread.Sleep(3000);
		}
	}
}
