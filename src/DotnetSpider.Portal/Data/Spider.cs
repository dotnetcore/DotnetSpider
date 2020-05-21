using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetSpider.Portal.Data
{
	[Table("SPIDER")]
	public class Spider
	{
		/// <summary>
		/// 主键
		/// </summary>
		[Column("ID")]
		public int Id { get; set; }

		/// <summary>
		/// 是否启用
		/// </summary>
		[Column("ENABLED")]
		public bool Enabled { get; set; }

		/// <summary>
		/// 爬虫名称
		/// </summary>
		[Required]
		[StringLength(255)]
		[Column("NAME")]
		public string Name { get; set; }

		/// <summary>
		/// 爬虫名称
		/// </summary>
		[Required]
		[StringLength(255)]
		[Column("IMAGE")]
		public string Image { get; set; }

		/// <summary>
		/// 定时表达式
		/// </summary>
		[StringLength(255)]
		[Required]
		[Column("CRON")]
		public string Cron { get; set; }

		/// <summary>
		/// docker 运行的环境变量
		/// </summary>
		[StringLength(2000)]
		[Column("ENVIRONMENT")]
		public string Environment { get; set; }

		/// <summary>
		/// docker 运行挂载的盘
		/// </summary>
		[StringLength(2000)]
		[Column("VOLUME")]
		public string Volume { get; set; }

		/// <summary>
		/// Creation time of this entity.
		/// </summary>
		[Required]
		[Column("CREATION_TIME")]
		public DateTimeOffset CreationTime { get; set; }

		/// <summary>
		/// 上一次更新时间
		/// </summary>
		[Column("LAST_MODIFICATION_TIME")]
		public DateTimeOffset LastModificationTime { get; set; }
	}
}
