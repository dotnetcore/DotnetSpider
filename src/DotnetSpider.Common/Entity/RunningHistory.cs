using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotnetSpider.Common.Entity
{
	/// <summary>
	/// 需要运行的任务实例
	/// </summary>
	public class RunningHistory
	{
		/// <summary>
		/// 任务实例标识
		/// </summary>
		public string Identity { get; set; }

		/// <summary>
		/// 任务的优先级
		/// </summary>
		public int Priority { get; set; }

		/// <summary>
		/// 实例创建时间
		/// </summary>
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// 修改时间
		/// </summary>
		public DateTime LastModificationTime { get; set; }
	}
}
