using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.AgentRegister.Store
{
	[Table("agent_heartbeat")]
	public class AgentHeartbeat
	{
		/// <summary>
		/// 节点标识
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// 标识
		/// </summary>
		[StringLength(36)]
		[Column("agent_id")]
		public string AgentId { get; private set; }

		/// <summary>
		/// 名称
		/// </summary>
		[StringLength(255)]
		[Column("agent_name")]
		public string AgentName { get; private set; }

		/// <summary>
		/// 空闲内存
		/// </summary>
		[Column("free_memory")]
		public int FreeMemory { get; private set; }

		[Column("cpu_load")]
		public int CpuLoad { get; private set; }

		/// <summary>
		/// 上报时间
		/// </summary>
		[Column("creation_time")]
		public DateTimeOffset CreationTime { get; private set; }

		public AgentHeartbeat(string agentId, string agentName, int freeMemory, int cpuLoad)
		{
			agentId.NotNullOrWhiteSpace(nameof(agentId));

			AgentId = agentId;
			AgentName = agentName;
			FreeMemory = freeMemory;
			CpuLoad = cpuLoad;
			CreationTime = DateTimeOffset.Now;
		}
	}
}
