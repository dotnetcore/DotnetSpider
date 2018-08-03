using DotnetSpider.Common;
using DotnetSpider.Common.Dto;
using DotnetSpider.Common.Entity;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	/// <summary>
	/// 节点服务接口
	/// </summary>
	public interface INodeService
	{
		/// <summary>
		/// 刷新节点的状态
		/// </summary>
		/// <param name="heartbeat">节点心跳</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		Task Heartbeat(NodeHeartbeatInput heartbeat);

		/// <summary>
		/// 添加或更新节点信息
		/// </summary>
		/// <param name="node">节点信息</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		Task AddOrUpdateNode(Node node);

		/// <summary>
		/// 删除节点信息
		/// </summary>
		/// <param name="nodeId">节点标识</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		Task RemoveNode(string nodeId);
	}
}
