using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetSpider.Portal.Data
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
		/// 是否启用
		/// </summary>
		public bool Enable { get; set; }

		/// <summary>
		/// 爬虫名称
		/// </summary>
		[Required]
		[StringLength(255)]
		[Column("name")]
		public string Name { get; set; }

		/// <summary>
		/// 需要运行的爬虫名称
		/// </summary>
		[Required]
		[StringLength(400)]
		[Column("type")]
		public string Type { get; set; }

		/// <summary>
		/// 定时表达式
		/// </summary>
		[StringLength(255)]
		[Required]
		[Column("cron")]
		public string Cron { get; set; }

		/// <summary>
		/// docker 运行的环境变量
		/// </summary>
		[StringLength(255)]
		[Column("environment")]
		public string Environment { get; set; }

		/// <summary>
		/// docker 镜像仓库地址
		/// </summary>
		[StringLength(255)]
		[Column("registry")]
		public string Registry { get; set; }

		/// <summary>
		/// docker 镜像仓库名称
		/// </summary>
		[StringLength(255)]
		[Required]
		[Column("repository")]
		public string Repository { get; set; }

		/// <summary>
		///  docker 镜像仓库标签
		/// </summary>
		[StringLength(255)]
		[Required]
		[Column("tag")]
		public string Tag { get; set; }

		/// <summary>
		/// Creation time of this entity.
		/// </summary>
		[Required]
		[Column("creation_time")]
		public DateTimeOffset CreationTime { get; set; }

		/// <summary>
		/// 上一次更新时间
		/// </summary>
		[Column("last_modification_time")]
		public DateTimeOffset LastModificationTime { get; set; }
	}
}
