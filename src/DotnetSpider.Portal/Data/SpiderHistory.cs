using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetSpider.Portal.Data
{
	[Table("SPIDER_HISTORIES")]
	public class SpiderHistory
	{
		/// <summary>
		/// 主键
		/// </summary>
		[Column("ID")]
		public int Id { get; set; }

		/// <summary>
		///
		/// </summary>
		[Column("SPIDER_ID")]
		[Required]
		public int SpiderId { get; set; }

		/// <summary>
		///
		/// </summary>
		[Column("SPIDER_NAME")]
		[StringLength(255)]
		[Required]
		public string SpiderName { get; set; }

		/// <summary>
		/// 容器标识
		/// </summary>
		[Column("CONTAINER_ID")]
		[StringLength(100)]
		public string ContainerId { get; set; }

		/// <summary>
		/// 容器标识
		/// </summary>
		[Column("BATCH")]
		[StringLength(36)]
		public string Batch { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		[Column("CREATION_TIME")]
		[Required]
		public DateTimeOffset CreationTime { get; set; }

		[Column("STATUS")]
		[StringLength(20)]
		[Required]
		public string Status { get; set; }
	}
}
