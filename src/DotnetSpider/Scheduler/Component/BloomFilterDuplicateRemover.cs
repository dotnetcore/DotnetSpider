using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Scheduler.Component
{
    /// <summary>
    /// BloomFilterDuplicateRemover for huge number of urls.
    /// </summary>
    public class BloomFilterDuplicateRemover : IDuplicateRemover
    {
        private BloomFilter _bloomFilter;
        private long _counter;
        private readonly BloomFilterOptions _options;

        /// <summary>
        /// Get TotalRequestsCount.
        /// </summary>
        /// <returns>TotalRequestsCount</returns>
        public long Total => _counter;

        /// <summary>
        /// 构造方法
        /// </summary>
        public BloomFilterDuplicateRemover(BloomFilterOptions options)
        {
            _options = options;
            _bloomFilter = new BloomFilter(_options.FalsePositiveProbability, _options.ExpectedInsertions);
            _counter = 0;
        }

        /// <summary>
        /// Check whether the request is duplicate.
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Whether the request is duplicate.</returns>
        public Task<bool> IsDuplicateAsync(Request request)
        {
            request.NotNull(nameof(request));
            request.Owner.NotNullOrWhiteSpace(nameof(request.Owner));
            
            var isDuplicate = _bloomFilter.Contains(request.Hash);
            if (!isDuplicate)
            {
                _bloomFilter.Add(request.Hash);
                Interlocked.Increment(ref _counter);
            }

            return Task.FromResult(isDuplicate);
        }

        /// <summary>
        /// Reset duplicate check.
        /// </summary>
        public void ResetDuplicateCheck()
        {
            _counter = Interlocked.Exchange(ref _counter, 0);
            _bloomFilter.Clear();
            _bloomFilter = new BloomFilter(_options.FalsePositiveProbability, _options.ExpectedInsertions);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _bloomFilter.Clear();
        }
    }
}