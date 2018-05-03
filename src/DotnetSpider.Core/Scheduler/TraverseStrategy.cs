using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// 遍历策略
	/// </summary>
	public enum TraverseStrategy
	{
		/// <summary>
		/// 深度优先
		/// </summary>
		DFS,
		/// <summary>
		/// 广度优先
		/// </summary>
		BFS
	}
}
