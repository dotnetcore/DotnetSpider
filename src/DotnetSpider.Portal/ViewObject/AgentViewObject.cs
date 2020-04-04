namespace DotnetSpider.Portal.ViewObject
{
	public class AgentViewObject
	{
		/// <summary>
		/// 标识
		/// </summary>
		public virtual string Id { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// CPU 核心数
		/// </summary>
		public int ProcessorCount { get; set; }

		/// <summary>
		/// 总内存
		/// </summary>
		public int TotalMemory { get; set; }

		/// <summary>
		/// 上一次更新时间
		/// </summary>
		public string LastModificationTime { get; set; }

		/// <summary>
		/// 是否已经标记删除
		/// </summary>
		public bool IsDeleted { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public string CreationTime { get; set; }

		public bool Online { get; set; }
	}
}
