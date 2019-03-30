using System;
using System.Threading.Tasks;
using DotnetSpider.Downloader;
using DotnetSpider.Scheduler;
using Xunit;

namespace DotnetSpider.Tests.Scheduler
{
    public class QueueSchedulerTests
    {
        [Fact(DisplayName = "ParallelEnqueueAndDequeueQueueBfs")]
        public void ParallelEnqueueAndDequeueQueueBfs()
        {
            var scheduler = new QueueDistinctBfsScheduler();
            var ownerId = Guid.NewGuid().ToString("N");
            Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 20}, i =>
            {
                scheduler.Enqueue(new[]
                {
                    new Request("http://www.a.com")
                    {
                        OwnerId = ownerId
                    }
                });
                scheduler.Enqueue(new[]
                {
                    new Request("http://www.a.com")
                    {
                        OwnerId = ownerId
                    }
                });
                scheduler.Enqueue(new[]
                {
                    new Request("http://www.a.com")
                    {
                        OwnerId = ownerId
                    }
                });
                scheduler.Enqueue(new[]
                {
                    new Request("http://www.b.com")
                    {
                        OwnerId = ownerId
                    }
                });
                scheduler.Enqueue(new[]
                {
                    new Request($"http://www.{i.ToString()}.com", null)
                    {
                        OwnerId = ownerId
                    }
                });
            });
            Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 20},
                i => { scheduler.Dequeue(ownerId); });

            Assert.Equal(2, scheduler.Requests[ownerId].Count);
            Assert.Equal(1002, scheduler.Total);
        }

        [Fact(DisplayName = "EnqueueAndDequeueQueueBfs")]
        public void EnqueueAndDequeueQueueBfs()
        {
            var ownerId = Guid.NewGuid().ToString("N");
            QueueDistinctBfsScheduler scheduler = new QueueDistinctBfsScheduler();
            scheduler.Enqueue(new[]
            {
                new Request("http://www.a.com")
                {
                    OwnerId = ownerId
                }
            });
            scheduler.Enqueue(new[]
            {
                new Request("http://www.b.com")
                {
                    OwnerId = ownerId
                }
            });
            scheduler.Enqueue(new[]
            {
                new Request("http://www.a.com")
                {
                    OwnerId = ownerId
                }
            });
            scheduler.Enqueue(new[]
            {
                new Request("http://www.a.com")
                {
                    OwnerId = ownerId
                }
            });

            var request = scheduler.Dequeue(ownerId)[0];
            Assert.Equal("http://www.a.com", request.Url);
            Assert.Single(scheduler.Requests[ownerId]);
            Assert.Equal(2, scheduler.Total);
        }
        
        [Fact(DisplayName = "EnqueueAndDequeueQueueDfs")]
        public void EnqueueAndDequeueQueueDfs()
        {
            var ownerId = Guid.NewGuid().ToString("N");
            QueueDistinctDfsScheduler scheduler = new QueueDistinctDfsScheduler();
            scheduler.Enqueue(new[]
            {
                new Request("http://www.a.com")
                {
                    OwnerId = ownerId
                }
            });
            scheduler.Enqueue(new[]
            {
                new Request("http://www.a.com")
                {
                    OwnerId = ownerId
                }
            });
            scheduler.Enqueue(new[]
            {
                new Request("http://www.a.com")
                {
                    OwnerId = ownerId
                }
            });
            scheduler.Enqueue(new[]
            {
                new Request("http://www.b.com")
                {
                    OwnerId = ownerId
                }
            });

            var request = scheduler.Dequeue(ownerId)[0];
            Assert.Equal("http://www.b.com", request.Url);
            Assert.Single(scheduler.Requests[ownerId]);
            Assert.Equal(2, scheduler.Total);
        }

        [Fact(DisplayName = "ParallelEnqueueAndDequeueQueueDfs")]
        public void ParallelEnqueueAndDequeueQueueDfs()
        {
            var scheduler = new QueueDistinctDfsScheduler();
            var ownerId = Guid.NewGuid().ToString("N");
            Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 20}, i =>
            {
                scheduler.Enqueue(new[]
                {
                    new Request("http://www.a.com")
                    {
                        OwnerId = ownerId
                    }
                });
                scheduler.Enqueue(new[]
                {
                    new Request("http://www.a.com")
                    {
                        OwnerId = ownerId
                    }
                });
                scheduler.Enqueue(new[]
                {
                    new Request("http://www.a.com")
                    {
                        OwnerId = ownerId
                    }
                });
                scheduler.Enqueue(new[]
                {
                    new Request("http://www.b.com")
                    {
                        OwnerId = ownerId
                    }
                });
                scheduler.Enqueue(new[]
                {
                    new Request($"http://www.{i.ToString()}.com", null)
                    {
                        OwnerId = ownerId
                    }
                });
            });
            Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 20},
                i => { scheduler.Dequeue(ownerId); });

            Assert.Equal(2, scheduler.Requests[ownerId].Count);
            Assert.Equal(1002, scheduler.Total);
        }
    }
}