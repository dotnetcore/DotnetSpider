using DotnetSpider.Common;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Common.Entity;
using DotnetSpider.Common.Dto;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Broker.Services.MySql
{
	/// <summary>
	/// 节点服务的实现
	/// </summary>
	public class NodeService : Services.NodeService
	{
		public NodeService(BrokerOptions options, ILogger<BlockService> logger) : base(options, logger)
		{
		}

		/// <summary>
		/// 添加或更新节点信息
		/// </summary>
		/// <param name="node">节点信息</param>
		/// <returns>A System.Threading.Tasks.Task that represents the asynchronous invoke.</returns>
		public override async Task AddOrUpdate(Node node)
		{
			using (var conn = CreateDbConnection())
			{
				// node_id 是主键, 如果插入成功则为第一次插入
				bool success = (await conn.ExecuteAsync($"INSERT IGNORE INTO dotnetspider.node (node_id,ip,cpu_count,is_enable,{LeftEscapeSql}group{RightEscapeSql},os,totalMemory) VALUES (@NodeId,@Ip,@CpuCount,true,@Group,@Os,@TotalMemory);", node)) == 1;
				// 插入不成功则做更新操作
				if (!success)
				{
					// 节点注册时不能更新 is_enable 值, 此值需要由用户管理 
					await conn.ExecuteAsync($"UPDATE dotnetspider.node SET ip=@Ip,cpucount=@CpuCount{(node.IsEnable.HasValue ? ",isenable = @IsEnable" : "")},{LeftEscapeSql}group{RightEscapeSql}=@Group,os=@Os,totalMemory=@TotalMemory WHERE nodeid=@NodeId;", node);
				}
			}
		}

		protected override string LeftEscapeSql => "`";

		protected override string RightEscapeSql => "`";

		protected override string GetDateSql => "current_timestamp()";
	}
}
