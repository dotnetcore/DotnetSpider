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
		private readonly ConcurrentDictionary<string, object> _dict = new();
		private string _spiderId;

		/// <summary>
		/// Check whether the request is duplicate.
		/// </summary>
		/// <param name="request">Request</param>
		/// <returns>Whether the request is duplicate.</returns>
		public Task<bool> IsDuplicateAsync(Request request)
		{
			request.NotNull(nameof(request));
			request.Owner.NotNullOrWhiteSpace(nameof(request.Owner));

			if (request.Owner != _spiderId)
			{
				throw new SpiderException("请求所属爬虫的标识与去重器所属的爬虫标识不一致");
			}

			var isDuplicate = _dict.TryAdd(request.Hash, null);
			return Task.FromResult(!isDuplicate);
		}

		public Task InitializeAsync(string spiderId)
		{
			spiderId.NotNullOrWhiteSpace(nameof(spiderId));
			_spiderId = spiderId;
			return Task.CompletedTask;
		}

		public Task<long> GetTotalAsync()
		{
			return Task.FromResult((long)_dict.Count);
		}

		/// <summary>
		/// 重置去重器
		/// </summary>
		public Task ResetDuplicateCheckAsync()
		{
			_dict.Clear();
			return Task.CompletedTask;
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
