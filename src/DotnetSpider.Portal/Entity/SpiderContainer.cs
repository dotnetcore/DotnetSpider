using System;
using System.ComponentModel.DataAnnotations;
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
		[Required]
		public int SpiderId { get; set; }

		/// <summary>
		/// 容器标识
		/// </summary>
		[Column("container_id")]
		[StringLength(100)]
		public string ContainerId { get; set; }

		/// <summary>
		/// 容器标识
		/// </summary>
		[Column("batch")]
		[StringLength(100)]
		public string Batch { get; set; }
		
		/// <summary>
		/// 创建时间
		/// </summary>
		[Column("creation_time")]
		[Required]
		public DateTime CreationTime { get; set; }

		[Column("status")]
		[StringLength(20)]
		[Required]
		public string Status { get; set; }
	}
}