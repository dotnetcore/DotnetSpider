using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DotnetSpider.Common.Entity
{
	/// <summary>
	/// 需要运行的任务实例
	/// </summary>
	public class Running
	{
		[Key]
		[StringLength(32)]
		/// <summary>
		/// 任务实例标识
		/// </summary>
		public string Identity { get; set; }

		/// <summary>
		/// 此实例线程数
		/// </summary>
		public int ThreadNum { get; set; } = 1;

		/// <summary>
		/// 此实例的 Site 对象序列化
		/// </summary>
		public string Site { get; set; }

		/// <summary>
		/// 任务的优先级
		/// </summary>
		public int Priority { get; set; }

		/// <summary>
		/// Block 次数
		/// </summary>
		public int BlockTimes { get; set; }

		/// <summary>
		/// 实例创建时间
		/// </summary>
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// 修改时间
		/// </summary>
		public DateTime? LastModificationTime { get; set; }
	}
}
