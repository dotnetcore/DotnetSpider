using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DotnetSpider.Portal.Entity
{
	[Table("spider")]
	public class Spider
	{
		/// <summary>
		/// 主键
		/// </summary>
		[Column("id")]
		public int Id { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		[StringLength(255)]
		[Column("name")]
		public string Name { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		[Required]
		[Column("cron")]
		public string Cron { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		[Column("arguments")]
		public string Arguments { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		[Required]
		[Column("image")]
		public string Image { get; set; }

		/// <summary>
		/// Creation time of this entity.
		/// </summary>
		[Required]
		[Column("creation_time")]
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// 上一次更新时间
		/// </summary>
		[Column("last_modification_time")]
		public DateTime LastModificationTime { get; set; }
	}
}