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

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="spiderId"></param>
		Task InitializeAsync(string spiderId);

		/// <summary>
		/// 获取总数
		/// </summary>
		Task<long> GetTotalAsync();

		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		Task ResetDuplicateCheckAsync();
	}
}
