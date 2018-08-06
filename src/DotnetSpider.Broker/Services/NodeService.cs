using DotnetSpider.Common;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Common.Entity;
using DotnetSpider.Common.Dto;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Broker.Services
{
	/// <summary>
	/// 节点服务的实现
	/// </summary>
	public class NodeService : BaseService, INodeService
	{
		public NodeService(BrokerOptions options, ILogger<BlockService> logger) : base(options, logger)
		{
		}

		/// <summary>
		/// 添加或更新节点信息
		/// </summary>
		/// <param name="node">节点信息</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		public virtual async Task AddOrUpdate(Node node)
		{
			using (var conn = CreateDbConnection())
			{
				// node_id 是主键, 如果插入成功则为第一次插入
				bool success = (await conn.ExecuteAsync(
					$"IF(SELECT nodeid FROM node WHERE nodeid=@NodeId) IS NULL INSERT INTO node (nodeid,ip,cpucount,isenable,{LeftEscapeSql}group{RightEscapeSql},os,totalMemory,creationtime,lastmodificationtime) VALUES (@NodeId,@Ip,@CpuCount,'True',@Group,@Os,@TotalMemory,{GetDateSql},{GetDateSql});", node)) == 1;
				// 插入不成功则做更新操作
				if (!success)
				{
					// 节点注册时不能更新 is_enable 值, 此值需要由用户管理 
					await conn.ExecuteAsync($"UPDATE node SET ip=@Ip,cpucount=@CpuCount{(node.IsEnable.HasValue ? ",isenable = @IsEnable" : "")},{LeftEscapeSql}group{RightEscapeSql}=@Group,os=@Os,totalMemory=@TotalMemory, lastmodificationtime = {GetDateSql} WHERE nodeid=@NodeId;", node);
				}
			}
		}

		/// <summary>
		/// 查询节点最新的心跳
		/// </summary>
		/// <param name="nodeId">节点标识</param>
		/// <returns>心跳</returns>
		public async Task<NodeHeartbeat> GetLastHeartbeat(string nodeId)
		{
			using (var conn = CreateDbConnection())
			{
				return await conn.QueryFirstOrDefaultAsync<NodeHeartbeat>("SELECT * FROM nodeheartbeat WHERE nodeid = @NodeId;", new { NodeId = nodeId });
			}
		}

		/// <summary>
		/// 查询节点信息
		/// </summary>
		/// <param name="nodeId">节点标识</param>
		/// <returns>节点信息</returns>
		public virtual async Task<Node> Get(string nodeId)
		{
			using (var conn = CreateDbConnection())
			{
				return await conn.QueryFirstOrDefaultAsync<Node>("SELECT * FROM node WHERE nodeid = @NodeId;", new { NodeId = nodeId });
			}
		}

		/// <summary>
		/// 更新心跳
		/// </summary>
		/// <param name="heartbeat">节点心跳</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		public virtual async Task Heartbeat(NodeHeartbeatInput heartbeat)
		{
			using (var conn = CreateDbConnection())
			{
				await AddOrUpdate(new Node { CpuCount = heartbeat.CpuCount, Group = heartbeat.Group, Ip = heartbeat.Ip, NodeId = heartbeat.NodeId, Os = heartbeat.Os, TotalMemory = heartbeat.TotalMemory });
				await conn.ExecuteAsync($"INSERT INTO nodeheartbeat (nodeid,cpu,freememory,processcount) VALUES (@NodeId,@Cpu,@FreeMemory,{(heartbeat.Runnings == null ? 0 : heartbeat.Runnings.Length)});", heartbeat);
			}
		}

		/// <summary>
		/// 删除节点信息
		/// </summary>
		/// <param name="nodeId">节点标识</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		public virtual async Task Remove(string nodeId)
		{
			using (var conn = CreateDbConnection())
			{
				await conn.ExecuteAsync("DELETE FROM node WHERE nodeid = @NodeId;", new { NodeId = nodeId });
			}
		}
	}
}
