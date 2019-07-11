using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DotnetSpider.DownloadAgentRegisterCenter.Entity
{
	/// <summary>
	/// 下载器代理
	/// </summary>
	[Table("downloader_agent")]
	public class DownloaderAgent
	{
		/// <summary>
		/// 标识
		/// </summary>
		[Column("id")]
		[Key]
		[StringLength(40)]
		public string Id { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		[Column("name")]
		[StringLength(255)]
		public string Name { get; set; }

		/// <summary>
		/// CPU 核心数
		/// </summary>
		[Column("processor_count")]
		public int ProcessorCount { get; set; }

		/// <summary>
		/// 总内存
		/// </summary>
		[Column("total_memory")]
		public int TotalMemory { get; set; }

		/// <summary>
		/// 上一次更新时间
		/// </summary>
		[Column("last_modification_time")]
		public DateTime LastModificationTime { get; set; }

		/// <summary>
		/// 是否已经标记删除
		/// </summary>
		[Column("is_deleted")]
		public bool IsDeleted { get; set; }
		
		/// <summary>
		/// 创建时间
		/// </summary>
		[Column("creation_time")]
		[Required]
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// 刷新上一次更新时间
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void RefreshLastModificationTime()
		{
			LastModificationTime = DateTime.Now;
		}

		public bool IsActive()
		{
			return (DateTime.Now - LastModificationTime).TotalSeconds <= 30;
		}
	}
}