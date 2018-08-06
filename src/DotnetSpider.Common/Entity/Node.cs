using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DotnetSpider.Common.Entity
{
	/// <summary>
	/// 节点信息
	/// </summary>
	public class Node
	{
		/// <summary>
		/// 节点标识
		/// </summary>
		[Key]
		[Required]
		[StringLength(32)]
		public string NodeId { get; set; }

		/// <summary>
		/// IP 地址
		/// </summary>
		[StringLength(32)]
		public string Ip { get; set; }

		/// <summary>
		/// CPU 核心数
		/// </summary>
		public int CpuCount { get; set; }

		/// <summary>
		/// 分组
		/// </summary>
		[Required]
		[StringLength(32)]
		public string Group { get; set; }

		/// <summary>
		/// 操作系统
		/// </summary>
		[Required]
		[StringLength(32)]
		public string Os { get; set; }

		/// <summary>
		/// 系统内存
		/// </summary>
		public int TotalMemory { get; set; }

		/// <summary>
		/// 是否启用
		/// </summary>
		public bool? IsEnable { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// 修改时间
		/// </summary>
		public DateTime? LastModificationTime { get; set; }
	}
}
