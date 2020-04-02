using System;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using Xunit;

namespace DotnetSpider.Tests
{
    public class QueueSchedulerTests
    {
        [Fact(DisplayName = "ParallelEnqueueAndDequeueQueueBfs")]
        public void ParallelEnqueueAndDequeueQueueBfs()
        {
            var scheduler = new QueueDistinctBfsScheduler(new HashSetDuplicateRemover());
            var ownerId = Guid.NewGuid().ToString("N");
            Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 20}, async i =>
            {
                await scheduler.EnqueueAsync(new[]
                {
                    new Request("http://www.a.com")
                    {
                        Owner = ownerId
                    }
                });
                await scheduler.EnqueueAsync(new[]
                {
                    new Request("http://www.a.com")
                    {
                        Owner = ownerId
                    }
                });
                await scheduler.EnqueueAsync(new[]
                {
                    new Request("http://www.a.com")
                    {
                        Owner = ownerId
                    }
                });
                await scheduler.EnqueueAsync(new[]
                {
                    new Request("http://www.b.com")
                    {
                        Owner = ownerId
                    }
                });
                await scheduler.EnqueueAsync(new[]
                {
                    new Request($"http://www.{i.ToString()}.com", null)
                    {
                        Owner = ownerId
                    }
                });
            });
            Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 20},
                async i => { await scheduler.DequeueAsync(); });

            Assert.Equal(1002, scheduler.Total);
        }

        [Fact(DisplayName = "EnqueueAndDequeueQueueBfs")]
        public async Task EnqueueAndDequeueQueueBfs()
        {
            var ownerId = Guid.NewGuid().ToString("N");
            var scheduler = new QueueDistinctBfsScheduler(new HashSetDuplicateRemover());
            await scheduler.EnqueueAsync(new[]
            {
                new Request("http://www.a.com")
                {
                    Owner = ownerId
                }
            });
            await scheduler.EnqueueAsync(new[]
            {
                new Request("http://www.b.com")
                {
                    Owner = ownerId
                }
            });
            await scheduler.EnqueueAsync(new[]
            {
                new Request("http://www.a.com")
                {
                    Owner = ownerId
                }
            });
            await scheduler.EnqueueAsync(new[]
            {
                new Request("http://www.a.com")
                {
                    Owner = ownerId
                }
            });

            var request = (await scheduler.DequeueAsync()).First();
            Assert.Equal("http://www.a.com/", request.RequestUri.ToString());
            Assert.Equal(2, scheduler.Total);
        }

        [Fact(DisplayName = "EnqueueAndDequeueQueueDfs")]
        public async Task EnqueueAndDequeueQueueDfs()
        {
            var ownerId = Guid.NewGuid().ToString("N");
            var scheduler = new QueueDistinctDfsScheduler(new HashSetDuplicateRemover());
            await scheduler.EnqueueAsync(new[]
            {
                new Request("http://www.a.com")
                {
                    Owner = ownerId
                }
            });
            await scheduler.EnqueueAsync(new[]
            {
                new Request("http://www.a.com")
                {
                    Owner = ownerId
                }
            });
            await scheduler.EnqueueAsync(new[]
            {
                new Request("http://www.a.com")
                {
                    Owner = ownerId
                }
            });
            await scheduler.EnqueueAsync(new[]
            {
                new Request("http://www.b.com")
                {
                    Owner = ownerId
                }
            });

            var request = (await scheduler.DequeueAsync()).First();
            Assert.Equal("http://www.b.com/", request.RequestUri.ToString());
            Assert.Equal(2, scheduler.Total);
        }

        [Fact(DisplayName = "ParallelEnqueueAndDequeueQueueDfs")]
        public Task ParallelEnqueueAndDequeueQueueDfs()
        {
            var scheduler = new QueueDistinctDfsScheduler(new HashSetDuplicateRemover());
            var ownerId = Guid.NewGuid().ToString("N");
            Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 10}, async i =>
            {
                await scheduler.EnqueueAsync(new[]
                {
                    new Request("http://www.a.com")
                    {
                        Owner = ownerId
                    }
                });
                await scheduler.EnqueueAsync(new[]
                {
                    new Request("http://www.a.com")
                    {
                        Owner = ownerId
                    }
                });
                await scheduler.EnqueueAsync(new[]
                {
                    new Request("http://www.a.com")
                    {
                        Owner = ownerId
                    }
                });
                await scheduler.EnqueueAsync(new[]
                {
                    new Request("http://www.b.com")
                    {
                        Owner = ownerId
                    }
                });
                await scheduler.EnqueueAsync(new[]
                {
                    new Request($"http://www.{i.ToString()}.com", null)
                    {
                        Owner = ownerId
                    }
                });
            });
            Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 20},
                async i => { await scheduler.DequeueAsync(); });
            
            Assert.Equal(1002, scheduler.Total);
            return Task.CompletedTask;
        }
    }
}