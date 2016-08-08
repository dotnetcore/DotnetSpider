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
		/// <param name="request"></param>
		/// <returns></returns>
		bool IsDuplicate(Request request);

		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		void ResetDuplicateCheck();

		/// <summary>
		/// Get TotalRequestsCount for monitor.
		/// </summary>
		/// <returns></returns>
		long GetTotalRequestsCount();
	}
}
