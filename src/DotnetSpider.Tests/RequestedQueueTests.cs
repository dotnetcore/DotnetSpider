using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using Xunit;

namespace DotnetSpider.Tests;

public class RequestedQueueTests
{
    [Fact]
    public void Enqueue()
    {
        var queue = new RequestedQueue();
        var request = new Request("http://www.baidu.com") {Timeout = 2000};
        var requestHasher = new RequestHasher(new MurmurHashAlgorithmService());
        requestHasher.ComputeHash(request);
        queue.Enqueue(request);
        Assert.Equal(1, queue.Count);
        queue.Enqueue(request);
        Assert.Equal(1, queue.Count);
        var request2 = new Request("http://www.baidu.com/2") {Timeout = 2000};
        requestHasher.ComputeHash(request2);
        queue.Enqueue(request2);
        Assert.Equal(2, queue.Count);
    }

    [Fact]
    public void DequeueTimeout()
    {
        var queue = new RequestedQueue();
        var request = new Request("http://www.baidu.com") {Timeout = 2000};
        var requestHasher = new RequestHasher(new MurmurHashAlgorithmService());
        requestHasher.ComputeHash(request);
        queue.Enqueue(request);
        Assert.Equal(1, queue.Count);
        Thread.Sleep(2500);
        Assert.Null(queue.Dequeue(request.Hash));
        var timeoutRequests = queue.GetAllTimeoutList();
        Assert.Single(timeoutRequests);
        Assert.Equal(request.Hash, timeoutRequests[0].Hash);
    }

    [Fact]
    public void Dequeue()
    {
        var queue = new RequestedQueue();
        var request = new Request("http://www.baidu.com") {Timeout = 2000};
        var requestHasher = new RequestHasher(new MurmurHashAlgorithmService());
        requestHasher.ComputeHash(request);
        queue.Enqueue(request);
        Assert.Equal(1, queue.Count);
        Thread.Sleep(1000);
        var request2 = queue.Dequeue(request.Hash);
        Assert.NotNull(request2);
        Assert.Equal(request, request2);
        Assert.Equal(request.Hash, request2.Hash);
    }

    [Fact]
    public void ParallelEnqueue()
    {
        var queue = new RequestedQueue();
        var requestHasher = new RequestHasher(new MurmurHashAlgorithmService());
        Parallel.For(1, 10000, new ParallelOptions(), (i) =>
        {
            var request = new Request($"http://www.baidu.com/{i}") {Timeout = 2000};
            requestHasher.ComputeHash(request);
            queue.Enqueue(request);
        });
    }

    [Fact]
    public void ParallelDequeue()
    {
        var queue = new RequestedQueue();
        var requestHasher = new RequestHasher(new MurmurHashAlgorithmService());
        var hashes = new List<string>();
        for (var i = 0; i < 10000; ++i)
        {
            var request = new Request($"http://www.baidu.com/{i}") {Timeout = 30000};
            requestHasher.ComputeHash(request);
            hashes.Add(request.Hash);
            queue.Enqueue(request);
        }

        Parallel.ForEach(hashes, new ParallelOptions(), (hash) =>
        {
            var request = queue.Dequeue(hash);
            Assert.NotNull(request);
        });
    }

    /// <summary>
    /// CRITICAL BUG TEST: When a request is dequeued (response received) and then
    /// a NEW request with the SAME hash is enqueued, the old timer should NOT
    /// affect the new request.
    ///
    /// Scenario:
    /// 1. Request A (hash X) enqueued - timer starts for 2s
    /// 2. Response for A received at T=1s - A dequeued successfully
    /// 3. Request B (same hash X) enqueued - new entry in dict
    /// 4. Timer for A fires at T=2s - should NOT remove B from dict!
    /// </summary>
    [Fact]
    public void SameHashReuse_OldTimerShouldNotAffectNewRequest()
    {
        var queue = new RequestedQueue();
        var requestHasher = new RequestHasher(new MurmurHashAlgorithmService());

        // Create the request A with 2s timeout
        var requestA = new Request("http://www.example.com/page")
        {
            Timeout = 2000,
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
        };
        requestHasher.ComputeHash(requestA);

        // T=0: Enqueue request A
        queue.Enqueue(requestA);
        Assert.Equal(1, queue.Count);

        // T=1s: Response for A arrives - dequeue A
        Thread.Sleep(1000);
        var dequeuedA = queue.Dequeue(requestA.Hash);
        Assert.NotNull(dequeuedA);
        Assert.Equal(0, queue.Count);

        // Immediately enqueue request B with the SAME hash
        // (In the real scenario, this could be a retry or same URL requested again)
        var requestB = new Request("http://www.example.com/page")
        {
            Timeout = 2000,
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
        };
        requestHasher.ComputeHash(requestB);

        queue.Enqueue(requestB);
        Assert.Equal(1, queue.Count);

        // T=2s: Timer for request A fires (1s after A was enqueued + 1s wait)
        // Wait for A's timer to fire (it was set for 2s from T=0, so 1s more)
        Thread.Sleep(1100);

        // EXPECTED CORRECT BEHAVIOR:
        // Request B should still be in the queue - A's timer should NOT affect it
        // B was only enqueued ~1s ago, so its timer shouldn't fire until ~3s from the start

        // Try to dequeue B - it should still be there
        var dequeuedB = queue.Dequeue(requestB.Hash);

        // With the bug: dequeuedB is null because A's timer removed B
        // After fix: dequeuedB is not null, B is still in the queue
        Assert.NotNull(dequeuedB);

        // Also, verify nothing is in a timeout list for B
        // (A completed successfully, B was just dequeued successfully)
        var timeoutList = queue.GetAllTimeoutList();
        Assert.Empty(timeoutList);
    }
}
