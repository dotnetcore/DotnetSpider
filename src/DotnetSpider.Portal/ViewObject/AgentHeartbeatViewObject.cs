namespace DotnetSpider.Portal.ViewObject
{
	public class AgentHeartbeatViewObject
	{
		/// <summary>
		/// 节点标识
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// 标识
		/// </summary>
		public string AgentId { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		public string AgentName { get; set; }

		/// <summary>
		/// 空闲内存
		/// </summary>
		public int AvailableMemory { get; set; }

		public int CpuLoad { get; set; }

		/// <summary>
		/// 上报时间
		/// </summary>
		public string CreationTime { get; set; }
	}
}
