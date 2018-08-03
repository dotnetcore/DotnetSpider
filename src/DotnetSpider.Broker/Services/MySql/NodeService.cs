using DotnetSpider.Common;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Common.Entity;
using DotnetSpider.Common.Dto;

namespace DotnetSpider.Broker.Services.MySql
{
	/// <summary>
	/// 节点服务的实现
	/// </summary>
	public class NodeService : BaseService, INodeService
	{
		public NodeService(BrokerOptions options) : base(options)
		{
		}

		/// <summary>
		/// 添加或更新节点信息
		/// </summary>
		/// <param name="node">节点信息</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		public async Task AddOrUpdateNode(Node node)
		{
			using (var conn = CreateDbConnection())
			{
				// node_id 是主键, 如果插入成功则为第一次插入
				bool success = (await conn.ExecuteAsync("INSERT IGNORE INTO dotnetspider.node (node_id,ip,cpu_count,is_enable,`group`,os,totalMemory) VALUES (@NodeId,@Ip,@CpuCount,true,@Group,@Os,@TotalMemory);", node)) == 1;
				// 插入不成功则做更新操作
				if (!success)
				{
					// 节点注册时不能更新 is_enable 值, 此值需要由用户管理 
					await conn.ExecuteAsync($"UPDATE dotnetspider.node SET ip=@Ip,cpu_count=@CpuCount{(node.IsEnable.HasValue ? ",is_enable = @IsEnable" : "")},`group`=@Group,os=@Os,totalMemory=@TotalMemory WHERE node_id=@NodeId;", node);
				}
			}
		}

		/// <summary>
		/// 更新心跳
		/// </summary>
		/// <param name="heartbeat">节点心跳</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		public async Task Heartbeat(NodeHeartbeatInput heartbeat)
		{
			using (var conn = CreateDbConnection())
			{
				await AddOrUpdateNode(new Node { CpuCount = heartbeat.CpuCount, Group = heartbeat.Group, Ip = heartbeat.Ip, NodeId = heartbeat.NodeId, Os = heartbeat.Os, TotalMemory = heartbeat.TotalMemory });
				await conn.ExecuteAsync("INSERT IGNORE INTO dotnetspider.node_heartbeat (node_id,cpu,free_memory) VALUES (@NodeId,@Cpu,@FreeMemory);", heartbeat);
			}
		}

		/// <summary>
		/// 删除节点信息
		/// </summary>
		/// <param name="nodeId">节点标识</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		public async Task RemoveNode(string nodeId)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync("DELETE FROM dotnetspider.node WHERE node_id = @NodeId;", new { NodeId = nodeId });
			}
		}
	}
}
