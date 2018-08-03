using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotnetSpider.Common.Dto
{
	/// <summary>
	/// 下载节点心跳
	/// </summary>
	public class NodeHeartbeatInput
	{
		/// <summary>
		/// 节点唯一标识
		/// </summary>
		public string NodeId { get; set; }

		/// <summary>
		/// 正在运行的任务标识
		/// </summary>
		public string[] Runnings { get; set; }

		/// <summary>
		/// CPU 使用率
		/// </summary>
		public int Cpu { get; set; }

		/// <summary>
		/// 空闲内存
		/// </summary>
		public long FreeMemory { get; set; }

		/// <summary>
		/// IP 地址
		/// </summary>
		//[StringLength(32)]
		public string Ip { get; set; }

		/// <summary>
		/// CPU 核心数
		/// </summary>
		public int CpuCount { get; set; }

		/// <summary>
		/// 分组
		/// </summary>
		//[Required]
		//[StringLength(32)]
		public string Group { get; set; }

		/// <summary>
		/// 操作系统
		/// </summary>
		//[Required]
		//[StringLength(32)]
		public string Os { get; set; }

		/// <summary>
		/// 系统内存
		/// </summary>
		public int TotalMemory { get; set; }

		public override string ToString()
		{
			return $"{CpuCount}, {TotalMemory}, {Cpu}, {FreeMemory}, {Runnings.Length}";
		}
	}
}
