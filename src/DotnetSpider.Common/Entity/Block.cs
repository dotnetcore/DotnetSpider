using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DotnetSpider.Common.Entity
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum BlockStatus
	{
		Ready = 1,
		Using = 2,
		Success = 3,
		Failed = 4,
		Retry = 5
	}

	/// <summary>
	/// 任务块信息
	/// </summary>
	public class Block
	{
		/// <summary>
		/// 块标识
		/// </summary>
		[Key]
		public string BlockId { get; set; }

		/// <summary>
		/// 实例标识
		/// </summary>
		[Key]
		public string Identity { get; set; }

		/// <summary>
		/// 如果符合正则， Downloader 执行切换 IP 操作
		/// </summary>
		public string ChangeIpPattern { get; set; }

		/// <summary>
		/// 执行异常信息
		/// </summary>
		public string Exception { get; set; }

		/// <summary>
		/// 状态
		/// </summary>
		public BlockStatus Status { get; set; }

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
