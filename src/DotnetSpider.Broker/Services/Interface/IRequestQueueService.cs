using DotnetSpider.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	/// <summary>
	/// 队列接口
	/// </summary>
	public interface IRequestQueueService
	{
		/// <summary>
		/// 添加请求到队列
		/// </summary>
		/// <param name="requests">请求</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		Task Add(IEnumerable<RequestQueue> requests);

		/// <summary>
		/// 添加请求到队列
		/// </summary>
		/// <param name="json">Request 数组的序列化</param>
		/// <param name="identity">实例标识</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		Task<string> Add(string identity, string json);

		/// <summary>
		/// 获取请求
		/// </summary>
		/// <param name="blockId">块标识</param>
		/// <returns>请求</returns>
		Task<IEnumerable<RequestQueue>> GetByBlockId(string blockId);
	}
}
