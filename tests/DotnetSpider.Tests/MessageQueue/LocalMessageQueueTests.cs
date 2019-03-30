using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.MessageQueue;
using Xunit;

namespace DotnetSpider.Tests.MessageQueue
{
    public class LocalMessageQueueTests : TestBase
    {
        [Fact(DisplayName = "PubAndSub")]
        public async Task PubAndSub()
        {
            int count = 0;
            var mq = new LocalMessageQueue(null);
            mq.Subscribe("topic", msg => { Interlocked.Increment(ref count); });
            for (int i = 0; i < 1000; ++i)
            {
                await mq.PublishAsync("topic", "a");
            }

            Thread.Sleep(2000);
            Assert.Equal(1000, count);
        }

        [Fact(DisplayName = "PubAndUnSub")]
        public async Task PubAndUnSub()
        {
            int count = 0;
            var mq = new LocalMessageQueue(null);
            mq.Subscribe("topic", msg => { Interlocked.Increment(ref count); });

            int i = 0;
            Task.Factory.StartNew(async () =>
            {
                for (; i < 100; ++i)
                {
                    await mq.PublishAsync("topic", "a");
                    await Task.Delay(100);
                }
            }).ConfigureAwait(false).GetAwaiter();
            await Task.Delay(3000);
            mq.Unsubscribe("topic");
            while (i < 100)
            {
                await Task.Delay(1000);
            }

            Assert.True(count < 100);
        }
    }
}