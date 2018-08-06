namespace DotnetSpider.Common.Entity
{
	/// <summary>
	/// 任务
	/// </summary>
	public class Job
	{
		/// <summary>
		/// 任务标识
		/// </summary>
		public virtual string Id { get; set; }

		/// <summary>
		/// 任务名称
		/// </summary>
		public virtual string Name { get; set; }

		/// <summary>
		/// 需要运行的程序
		/// </summary>
		public virtual string Application { get; set; }

		/// <summary>
		/// 附加参数
		/// </summary>
		public virtual string Arguments { get; set; }

		public virtual string Os { get; set; }

		/// <summary>
		/// Cron表达式
		/// </summary>
		public virtual string Cron { get; set; }

		/// <summary>
		/// 所需节点数
		/// </summary>
		public virtual int NodeCount { get; set; }

		public virtual string NodeType { get; set; }

		/// <summary>
		/// 描述
		/// </summary>
		public virtual string Description { get; set; }

		/// <summary>
		/// 程序包链接
		/// </summary>
		public virtual string Package { get; set; }

		/// <summary>
		/// 是否启用
		/// </summary>
		public virtual bool IsEnabled { get; set; }

		public virtual string Tags { get; set; }
	}
}
