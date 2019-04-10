using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetSpider.Portal.Entity
{
	[Table("spider_container")]
	public class SpiderContainer
	{
		/// <summary>
		/// 主键
		/// </summary>
		[Column("id")]
		public int Id { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Column("spider_id")]
		public int SpiderId { get; set; }

		/// <summary>
		/// 容器标识
		/// </summary>
		[Column("container_id")]
		public Guid ContainerId { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		[Column("creation_time")]
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// 退出时间
		/// </summary>
		[Column("exit_time")]
		public DateTime? ExitTime { get; set; }
	}
}