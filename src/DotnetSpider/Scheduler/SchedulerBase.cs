using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Scheduler.Component;

namespace DotnetSpider.Scheduler
{
    public abstract class SchedulerBase : IScheduler
    {
        protected readonly IDuplicateRemover DuplicateRemover;

        protected SchedulerBase(IDuplicateRemover duplicateRemover)
        {
            DuplicateRemover = duplicateRemover;
        }

        /// <summary>
        /// 重置去重器
        /// </summary>
        public virtual void ResetDuplicateCheck()
        {
            DuplicateRemover.ResetDuplicateCheck();
        }

        /// <summary>
        /// 如果请求未重复就添加到队列中
        /// </summary>
        /// <param name="request">请求</param>
        protected abstract Task PushWhenNoDuplicate(Request request);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            DuplicateRemover.Dispose();
        }

        /// <summary>
        /// 队列中的总请求个数
        /// </summary>
        public long Total => DuplicateRemover.Total;

        /// <summary>
        /// 从队列中取出指定爬虫的指定个数请求
        /// </summary>
        /// <param name="count">出队数</param>
        /// <returns>请求</returns>
        public abstract Task<IEnumerable<Request>> DequeueAsync(int count = 1);

        /// <summary>
        /// 请求入队
        /// </summary>
        /// <param name="requests">请求</param>
        /// <returns>入队个数</returns>
        public async Task<int> EnqueueAsync(IEnumerable<Request> requests)
        {
            var count = 0;
            foreach (var request in requests)
            {
                request.Hash = request.ComputeHash();
                if (!await DuplicateRemover.IsDuplicateAsync(request))
                {
                    await PushWhenNoDuplicate(request);
                    count++;
                }
            }

            return count;
        }
    }
}