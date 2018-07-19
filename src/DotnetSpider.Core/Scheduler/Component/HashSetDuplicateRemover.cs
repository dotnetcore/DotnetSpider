using DotnetSpider.Common;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Core.Scheduler.Component
{
	/// <summary>
	/// 通过哈希去重
	/// </summary>
	public class HashSetDuplicateRemover : IDuplicateRemover
	{
		private readonly HashSet<string> _urls = new HashSet<string>();

		/// <summary>
		/// Get TotalRequestsCount.
		/// </summary>
		/// <returns>TotalRequestsCount</returns>
		public long TotalRequestsCount => _urls.Count;

		/// <summary>
		/// Check whether the request is duplicate.
		/// </summary>
		/// <param name="request">Request</param>
		/// <returns>Whether the request is duplicate.</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool IsDuplicate(Request request)
		{
			bool isDuplicate = _urls.Contains(request.Identity);
			if (!isDuplicate)
			{
				_urls.Add(request.Identity);
			}
			return isDuplicate;
		}

		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ResetDuplicateCheck()
		{
			_urls.Clear();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_urls.Clear();
		}
	}
}
