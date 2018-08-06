using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DotnetSpider.Common
{
	/// <summary>
	/// 下载节点心跳
	/// </summary>
	public class NodeHeartbeat
	{
		/// <summary>
		/// 自增主键
		/// </summary>
		[Key]
		public int Id { get; set; }

		/// <summary>
		/// 节点唯一标识
		/// </summary>
		public string NodeId { get; set; }

		/// <summary>
		/// 任务运行数
		/// </summary>
		public int ProcessCount { get; set; }

		/// <summary>
		/// CPU 使用率
		/// </summary>
		public int Cpu { get; set; }

		/// <summary>
		/// 空闲内存
		/// </summary>
		public long FreeMemory { get; set; }
	}
}
