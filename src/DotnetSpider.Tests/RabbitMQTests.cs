using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using DotnetSpider.RabbitMQ;
using SwiftMQ;
using Xunit;

namespace DotnetSpider.Tests
{
	public class RabbitMQTests
	{
		protected virtual IMessageQueue GetMessageQueue()
		{
			return new RabbitMQMessageQueue(new RabbitMQOptions
			{
				Exchange = "MessageQueue", HostName = "localhost", UserName = "user", Password = "password"
			});
		}

		[Fact]
		public async Task Consumer()
		{
			var messageQueue = GetMessageQueue();
			var consumer = new AsyncMessageConsumer<byte[]>("test");
			consumer.Received += bytes => null;
			await messageQueue.ConsumeAsync(consumer, default);
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
				var message = (Message)MessagePack.MessagePackSerializer.Typeless.Deserialize(bytes);
				counter = message.Index;
				await Task.CompletedTask;
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
		}


		[Fact]
		public void ConcurrentConsume()
		{
			var messageQueue = GetMessageQueue();
			Parallel.For(0, 1000, async i =>
			{
				var consumer = new AsyncMessageConsumer<byte[]>("test");
				consumer.Received += bytes => null;
				await messageQueue.ConsumeAsync(consumer, default);
			});
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
				var message = (Message)MessagePack.MessagePackSerializer.Typeless.Deserialize(bytes);
				counter = message.Index;
				await Task.CompletedTask;
			};
			await messageQueue.ConsumeAsync(consumer, default);
			Sleep();
			await messageQueue.PublishAsBytesAsync(queue, new Message {Index = 1000});
			Sleep();
			Assert.Equal(1000, counter);
		}

		public class Message
		{
			public int Index { get; set; }
		}

		[Fact]
		public virtual void ConcurrentPublish()
		{
			var messageQueue = GetMessageQueue();
			var queue = Guid.NewGuid().ToString("N");
			var consumer = new AsyncMessageConsumer<byte[]>(queue);
			var counter = 0;
			consumer.Received += async bytes =>
			{
				counter += 1;
				await Task.CompletedTask;
			};

			var list = new List<Task>();
			for (var i = 0; i < 100; ++i)
			{
				var i1 = i;
				list.Add(Task.Factory.StartNew(async () =>
				{
					await messageQueue.PublishAsBytesAsync(queue, new Message {Index = i1});
				}));
			}

			Task.WaitAll(list.ToArray());

			Sleep();

			Assert.Equal(100, counter);
		}


		protected virtual void Sleep()
		{
			Thread.Sleep(3000);
		}
	}
}
