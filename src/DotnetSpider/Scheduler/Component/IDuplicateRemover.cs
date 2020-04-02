using System;
using System.Threading.Tasks;
using DotnetSpider.Http;

namespace DotnetSpider.Scheduler.Component
{
	public interface IDuplicateRemover : IDisposable
	{
		/// <summary>
		/// Check whether the request is duplicate.
		/// </summary>
		/// <param name="request">Request</param>
		/// <returns>Whether the request is duplicate.</returns>
		Task<bool> IsDuplicateAsync(Request request);

		long Total { get; }

		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		void ResetDuplicateCheck();
	}
}