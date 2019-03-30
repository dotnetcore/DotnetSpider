using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DotnetSpider.Data;
using DotnetSpider.Downloader;

namespace DotnetSpider.Scheduler
{
    /// <summary>
    /// 基于内存的深度优先调度(不去重 URL)
    /// </summary>
    public class QueueDfsScheduler : DuplicateRemovedScheduler
    {
        private readonly ConcurrentDictionary<string, List<Request>> _requests =
            new ConcurrentDictionary<string, List<Request>>();

        /// <summary>
        /// 构造方法
        /// </summary>
        public QueueDfsScheduler()
        {
            DuplicateRemover = new FakeDuplicateRemover();
        }
        
        /// <summary>
        /// 重置去重器
        /// </summary>
        public override void ResetDuplicateCheck()
        {
            DuplicateRemover.ResetDuplicateCheck();
        }
        
        /// <summary>
        /// 如果请求未重复就添加到队列中
        /// </summary>
        /// <param name="request">请求</param>
        protected override void PushWhenNoDuplicate(Request request)
        {
            if (!_requests.ContainsKey(request.OwnerId))
            {
                _requests.TryAdd(request.OwnerId, new List<Request>());
            }

            _requests[request.OwnerId].Add(request);
        }
        
        /// <summary>
        /// 从队列中取出指定爬虫的指定个数请求
        /// </summary>
        /// <param name="ownerId">爬虫标识</param>
        /// <param name="count">出队数</param>
        /// <returns>请求</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override Request[] Dequeue(string ownerId, int count = 1)
        {
            Check.NotNull(ownerId, nameof(ownerId));
            if (!_requests.ContainsKey(ownerId))
            {
                return new Request[0];
            }

            var dequeueCount = count;
            int start;
            if (_requests[ownerId].Count < count)
            {
                dequeueCount = _requests[ownerId].Count;
                start = 0;
            }
            else
            {
                start = _requests[ownerId].Count - dequeueCount - 1;
            }

            var requests = new List<Request>();
            for (int i = _requests.Count - 1; i >= start; --i)
            {
                requests.Add(_requests[ownerId][i]);
            }

            if (dequeueCount > 0)
            {
                _requests[ownerId].RemoveRange(start, dequeueCount);
            }

            return requests.ToArray();
        }
    }
}