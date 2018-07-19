using DotnetSpider.Common;
using System;

namespace DotnetSpider.Core.Scheduler.Component
{
	/// <summary>
	/// Remove duplicate requests.
	/// </summary>
	public interface IDuplicateRemover : IDisposable
	{
		/// <summary>
		/// Check whether the request is duplicate.
		/// </summary>
		/// <param name="request">Request</param>
		/// <returns>Whether the request is duplicate.</returns>
		bool IsDuplicate(Request request);

		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		void ResetDuplicateCheck();

		/// <summary>
		/// Get TotalRequestsCount.
		/// </summary>
		/// <returns>TotalRequestsCount</returns>
		long TotalRequestsCount { get; }
	}
}
