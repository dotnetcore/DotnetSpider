using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace DotnetSpider.Broker.Data
{
	/// <summary>
	/// 爬虫状态
	/// </summary>
	[Flags]
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Status
	{
		/// <summary>
		/// 初始化
		/// </summary>
		Init = 1,

		/// <summary>
		/// 正在运行
		/// </summary>
		Running = 2,

		/// <summary>
		/// 暂停
		/// </summary>
		Paused = 4,

		/// <summary>
		/// 完成
		/// </summary>
		Finished = 8,

		/// <summary>
		/// 退出
		/// </summary>
		Exited = 16
	}

	public class JobStatus : Entity<Guid>, IHasModificationTime
	{
		public virtual Guid Identity { get; set; }
		public virtual Guid NodeId { get; set; }
		public virtual Status Status { get; set; }
		public virtual string Detail { get; set; }
		public virtual DateTime? LastModificationTime { get; set; }
	}
}
