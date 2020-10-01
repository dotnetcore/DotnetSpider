using System.Collections.Concurrent;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Scheduler.Component
{
    /// <summary>
    /// 通过哈希去重
    /// </summary>
    public class HashSetDuplicateRemover : IDuplicateRemover
    {
        private readonly ConcurrentDictionary<string, object> _dict = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Check whether the request is duplicate.
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Whether the request is duplicate.</returns>
        public Task<bool> IsDuplicateAsync(Request request)
        {
            request.NotNull(nameof(request));
            request.Owner.NotNullOrWhiteSpace(nameof(request.Owner));

            var isDuplicate = _dict.TryAdd(request.Hash, null);
            return Task.FromResult(!isDuplicate);
        }

        public long Total => _dict.Count;

        /// <summary>
        /// 重置去重器
        /// </summary>
        public void ResetDuplicateCheck()
        {
            _dict.Clear();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _dict.Clear();
        }
    }
}
