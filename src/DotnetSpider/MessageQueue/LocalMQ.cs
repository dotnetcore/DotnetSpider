using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DotnetSpider.MessageQueue;

public class LocalMQ : IMessageQueue
{
    private readonly ConcurrentDictionary<string, Channel<byte[]>> _channelDict =
        new();

    public async Task PublishAsync(string queue, byte[] message)
    {
        var channel = GetOrCreate(queue);
        await channel.Writer.WriteAsync(message);
    }

    public Task ConsumeAsync(AsyncMessageConsumer<byte[]> consumer)
    {
        if (consumer.Registered)
        {
            throw new ApplicationException("This consumer is already registered");
        }

        consumer.Register();

        var cancellationTokenSource = new CancellationTokenSource();
        consumer.OnClosing += _ =>
        {
            cancellationTokenSource.Cancel();
        };

        Task.Run(async () =>
        {
            var channel = GetOrCreate(consumer.Queue);
            await foreach (var bytes in channel.Reader.ReadAllAsync(cancellationTokenSource.Token))
            {
                _ = Task.Run(async () =>
                {
                    await consumer.InvokeAsync(bytes);
                }, cancellationTokenSource.Token).ConfigureAwait(true).GetAwaiter();
            }

            // while (await channel.Reader.WaitToReadAsync(cancellationToken))
            // {
            //     var bytes = await channel.Reader.ReadAsync(cancellationToken);
            //     Task.Factory.StartNew(async () =>
            //         {
            //             await consumer.InvokeAsync(bytes);
            //         }, cancellationToken)
            //         .ConfigureAwait(false).GetAwaiter();
            // }
        }, cancellationTokenSource.Token).ConfigureAwait(true).GetAwaiter();
        return Task.CompletedTask;
    }

    public void CloseQueue(string queue)
    {
        if (!_channelDict.TryRemove(queue, out var channel))
        {
            return;
        }

        channel.Writer.Complete();
    }

    public bool IsDistributed => false;

    public void Dispose()
    {
        foreach (var kv in _channelDict)
        {
            kv.Value.Writer.Complete();
        }

        _channelDict.Clear();
    }

    private Channel<byte[]> GetOrCreate(string topic)
    {
        return _channelDict.GetOrAdd(topic, _ =>
        {
            // 创建一个有界的 Channel
            // 新的消息将会等待， 直到有空间可用
            var options = new BoundedChannelOptions(5000) { FullMode = BoundedChannelFullMode.Wait };
            return Channel.CreateBounded<byte[]>(options);
        });
    }
}
