using DotnetSpider.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 使用日志记录运行记录
	/// </summary>
	public class LogExecuteRecord : IExecuteRecord
	{
		private static readonly ILogger Logger = DLog.GetLogger();

		/// <summary>
		/// 添加运行记录
		/// </summary>
		/// <param name="taskId">任务编号</param>
		/// <param name="name">任务名称</param>
		/// <param name="identity">任务标识</param>
		/// <returns>是否添加成功</returns>
		public bool Add(string taskId, string name, string identity)
		{
			if (string.IsNullOrWhiteSpace(taskId) || string.IsNullOrWhiteSpace(identity))
			{
				return true;
			}
			Logger.Log(identity, $"Execute task {taskId}, name {name}, identity {identity}.", Level.Info);
			return true;
		}


		/// <summary>
		/// 删除运行记录
		/// </summary>
		/// <param name="taskId">任务编号</param>
		/// <param name="name">任务名称</param>
		/// <param name="identity">任务标识</param>
		public void Remove(string taskId, string name, string identity)
		{
			if (string.IsNullOrWhiteSpace(taskId) || string.IsNullOrWhiteSpace(identity))
			{
				return;
			}
			Logger.Log(identity, $"Complete task {taskId}, name {name}, identity {identity}.", Level.Info);
		}
	}
}
