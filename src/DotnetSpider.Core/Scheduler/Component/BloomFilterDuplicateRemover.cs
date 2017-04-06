using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Scheduler.Component
{
	/// <summary>
	/// BloomFilterDuplicateRemover for huge number of urls.
	/// </summary>
	public class BloomFilterDuplicateRemover : IDuplicateRemover
	{
		private readonly int _expectedInsertions;
		private readonly BloomFilter _bloomFilter;
		private readonly double _fpp;
		private AtomicInteger _counter;

		public BloomFilterDuplicateRemover(int expectedInsertions)
			: this(expectedInsertions, 0.01)
		{
		}

		public BloomFilterDuplicateRemover(int expectedInsertions, double fpp)
		{
			_expectedInsertions = expectedInsertions;
			_fpp = fpp;
			_bloomFilter = RebuildBloomFilter();
		}

		protected BloomFilter RebuildBloomFilter()
		{
			_counter = new AtomicInteger(0);
			return new BloomFilter(_fpp, _expectedInsertions);
		}

		public bool IsDuplicate(Request request)
		{
			bool isDuplicate = _bloomFilter.Contains(request.Identity);
			if (!isDuplicate)
			{
				_bloomFilter.Add(request.Identity);
				_counter.Inc();
			}
			return isDuplicate;
		}

		public void ResetDuplicateCheck()
		{
			RebuildBloomFilter();
		}

		public long GetTotalRequestsCount()
		{
			return _counter.Value;
		}

		public void Dispose()
		{
			_bloomFilter.Clear();
		}
	}
}