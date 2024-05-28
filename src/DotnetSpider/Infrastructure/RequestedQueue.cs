using System;
using System.Collections.Concurrent;
#if NETSTANDARD2_0
using System.Threading;
#endif
using System.Threading.Tasks;
using DotnetSpider.Http;
using HWT;

namespace DotnetSpider.Infrastructure;

public class RequestedQueue : IDisposable
{
    private readonly ConcurrentDictionary<string, Request> _dict = new();

    private readonly HashedWheelTimer _timer = new(TimeSpan.FromSeconds(1)
        , 100000);

    private readonly ConcurrentBag<Request> _queue = new();

    public int Count => _dict.Count;

    public bool Enqueue(Request request)
    {
        if (request.Timeout < 2000)
        {
            throw new SpiderException("Timeout should not less than 2000 milliseconds");
        }

        if (!_dict.TryAdd(request.Hash, request))
        {
            return false;
        }

        _timer.NewTimeout(new TimeoutTask(this, request.Hash),
            TimeSpan.FromMilliseconds(request.Timeout));
        return true;
    }


    public Request Dequeue(string hash)
    {
        return _dict.TryRemove(hash, out var request) ? request : null;
    }

    public Request[] GetAllTimeoutList()
    {
        var data = _queue.ToArray();
        _queue.Clear();
        return data;
    }

    private void Timeout(string hash)
    {
        if (_dict.TryRemove(hash, out var request))
        {
            _queue.Add(request);
        }
    }

    private class TimeoutTask(RequestedQueue requestedQueue, string hash) : ITimerTask
    {
        public Task RunAsync(ITimeout timeout)
        {
            requestedQueue.Timeout(hash);
            return Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        _dict.Clear();
        _queue.Clear();
        _timer.Stop();
        _timer.Dispose();
    }
}
