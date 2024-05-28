using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.MessageQueue;
using Xunit;

namespace DotnetSpider.Tests;

public class LocalMqTests
{
    [Fact]
    public async Task Publish1()
    {
        var queue = new LocalMQ();

        for (var k = 0; k < 100; ++k)
        {
            await queue.PublishAsync("test", "test"u8.ToArray());
        }

        var consumer = new AsyncMessageConsumer<byte[]>("test");
        var i = 0;
        consumer.Received += _ =>
        {
            Interlocked.Increment(ref i);
            return Task.CompletedTask;
        };
        await queue.ConsumeAsync(consumer);
        await Task.Delay(1000);
        Assert.Equal(100, i);
    }

    [Fact]
    public async Task Publish2()
    {
        var queue = new LocalMQ();

        var consumer = new AsyncMessageConsumer<byte[]>("test");
        var i = 0;
        consumer.Received += _ =>
        {
            Interlocked.Increment(ref i);
            return Task.CompletedTask;
        };
        await queue.ConsumeAsync(consumer);
        for (var k = 0; k < 100; ++k)
        {
            await queue.PublishAsync("test", "test"u8.ToArray());
        }

        await Task.Delay(1000);
        Assert.Equal(100, i);
    }

    [Fact]
    public async Task Close()
    {
        var queue = new LocalMQ();

        var consumer = new AsyncMessageConsumer<byte[]>("test");
        var i = 0;
        consumer.Received += _ =>
        {
            Interlocked.Increment(ref i);
            return Task.CompletedTask;
        };
        await queue.ConsumeAsync(consumer);
        for (var k = 0; k < 100; ++k)
        {
            await queue.PublishAsync("test", "test"u8.ToArray());
        }

        await Task.Delay(1000);
        consumer.Close();
        await queue.PublishAsync("test", "test"u8.ToArray());
        await queue.PublishAsync("test", "test"u8.ToArray());
        await Task.Delay(1000);
        Assert.Equal(100, i);
    }

    [Fact]
    public async Task ConsumeBalance()
    {
        var queue = new LocalMQ();

        var consumer1 = new AsyncMessageConsumer<byte[]>("test");
        var i = 0;
        consumer1.Received += _ =>
        {
            Interlocked.Increment(ref i);
            return Task.CompletedTask;
        };
        var consumer2 = new AsyncMessageConsumer<byte[]>("test");
        var j = 0;
        consumer2.Received += _ =>
        {
            Interlocked.Increment(ref j);
            return Task.CompletedTask;
        };
        await queue.ConsumeAsync(consumer1);
        await queue.ConsumeAsync(consumer2);
        for (var k = 0; k < 100; ++k)
        {
            await queue.PublishAsync("test", "test"u8.ToArray());
        }

        await Task.Delay(1000);

        var t = i + j;
        Assert.Equal(100, t);
    }
}
