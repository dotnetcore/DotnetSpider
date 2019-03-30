using DotnetSpider.Core;
using DotnetSpider.Data;
using DotnetSpider.Downloader;

namespace DotnetSpider.Scheduler.Component
{
	/// <summary>
	/// BloomFilterDuplicateRemover for huge number of urls.
	/// </summary>
	public class BloomFilterDuplicateRemover : IDuplicateRemover
	{
		private readonly int _expectedInsertions;
		private BloomFilter _bloomFilter;
		private readonly double _falsePositiveProbability;
		private AtomicInteger _counter;

		/// <summary>
		/// Get TotalRequestsCount.
		/// </summary>
		/// <returns>TotalRequestsCount</returns>
		public int Total => _counter.Value;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="expectedNumberOfElements">元素个数</param>
		public BloomFilterDuplicateRemover(int expectedNumberOfElements)
			: this(0.01, expectedNumberOfElements)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="falsePositiveProbability">误判机率</param>
		/// <param name="expectedInsertions">元素个数</param>
		public BloomFilterDuplicateRemover(double falsePositiveProbability, int expectedInsertions)
		{
			_expectedInsertions = expectedInsertions;
			_falsePositiveProbability = falsePositiveProbability;
			_bloomFilter = CreateBloomFilter(_falsePositiveProbability, _expectedInsertions);
		}

		/// <summary>
		/// Check whether the request is duplicate.
		/// </summary>
		/// <param name="request">Request</param>
		/// <returns>Whether the request is duplicate.</returns>
		public bool IsDuplicate(Request request)
		{
			Check.NotNull(request.OwnerId,nameof(request.OwnerId));
			var hash = request.Hash;
			bool isDuplicate = _bloomFilter.Contains(hash);
			if (!isDuplicate)
			{
				_bloomFilter.Add(hash);
				_counter.Inc();
			}

			return isDuplicate;
		}

		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		public void ResetDuplicateCheck()
		{
			_bloomFilter = CreateBloomFilter(_falsePositiveProbability, _expectedInsertions);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_bloomFilter.Clear();
		}

		private BloomFilter CreateBloomFilter(double falsePositiveProbability, int expectedNumberOfElements)
		{
			_counter = new AtomicInteger(0);
			return new BloomFilter(falsePositiveProbability, expectedNumberOfElements);
		}
	}
}