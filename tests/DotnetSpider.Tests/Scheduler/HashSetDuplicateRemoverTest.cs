using System;
using System.Threading.Tasks;
using DotnetSpider.Downloader;
using DotnetSpider.Scheduler.Component;
using Xunit;

namespace DotnetSpider.Tests.Scheduler
{
    public class HashSetDuplicateRemoverTests
    {
        [Fact(DisplayName = "HashSetDuplicate")]
        public void HashSetDuplicate()
        {
            HashSetDuplicateRemover scheduler = new HashSetDuplicateRemover();

            var ownerId = Guid.NewGuid().ToString("N");
            var r1 = new Request("http://www.a.com")
            {
                OwnerId = ownerId
            };
            r1.ComputeHash();
            bool isDuplicate = scheduler.IsDuplicate(r1);

            Assert.False(isDuplicate);
            var r2 = new Request("http://www.a.com")
            {
                OwnerId = ownerId
            };
            r2.ComputeHash();
            isDuplicate = scheduler.IsDuplicate(r2);
            Assert.True(isDuplicate);
            var r3 = new Request("http://www.b.com")
            {
                OwnerId = ownerId
            };
            r3.ComputeHash();
            isDuplicate = scheduler.IsDuplicate(r3);
            Assert.False(isDuplicate);
            var r4 = new Request("http://www.b.com")
            {
                OwnerId = ownerId
            };
            r4.ComputeHash();

            isDuplicate = scheduler.IsDuplicate(r4);
            Assert.True(isDuplicate);
        }

        [Fact(DisplayName = "ParallelHashSetDuplicate")]
        public void ParallelHashSetDuplicate()
        {
            var ownerId = Guid.NewGuid().ToString("N");
            HashSetDuplicateRemover scheduler = new HashSetDuplicateRemover();
            var r1 = new Request("http://www.a.com")
            {
                OwnerId = ownerId
            };
            r1.ComputeHash();
            bool isDuplicate = scheduler.IsDuplicate(r1);

            Assert.False(isDuplicate);
            Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 30}, i =>
            {
                var r = new Request("http://www.a.com")
                {
                    OwnerId = ownerId
                };
                r.ComputeHash();
                isDuplicate = scheduler.IsDuplicate(r);
                Assert.True(isDuplicate);
            });
        }
    }
}