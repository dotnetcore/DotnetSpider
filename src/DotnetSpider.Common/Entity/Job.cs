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
		/// 任务实现完整类名
		/// </summary>
		public virtual string Processor { get; set; }

		/// <summary>
		/// 附加参数
		/// </summary>
		public virtual string Arguments { get; set; }

		/// <summary>
		/// Cron表达式
		/// </summary>
		public virtual string Cron { get; set; }

		/// <summary>
		/// 在 Scheduler.NET 中的任务标识
		/// </summary>
		public virtual string SchedulerNetId { get; set; }

		/// <summary>
		/// 描述
		/// </summary>
		public virtual string Description { get; set; }

		/// <summary>
		/// 是否启用
		/// </summary>
		public virtual bool IsEnabled { get; set; }
	}
}
