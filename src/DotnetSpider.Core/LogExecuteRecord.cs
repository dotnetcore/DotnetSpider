using DotnetSpider.Common;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 使用日志记录运行记录
	/// </summary>
	public class LogExecuteRecord : IExecuteRecord
	{
		private readonly ILogger _logger;

		public LogExecuteRecord(ILogger logger)
		{
			_logger = logger ?? throw new SpiderException($"{nameof(logger)} should not be null.");
		}

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
			var msg = string.IsNullOrWhiteSpace(name) ? $"Execute task {taskId}, identity {identity}." : $"Execute task {taskId}, name {name}, identity {identity}.";
			_logger.Information(msg, identity);
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
			var msg = string.IsNullOrWhiteSpace(name) ? $"Finish task {taskId}, identity {identity}." : $"Finish task {taskId}, name {name}, identity {identity}.";
			_logger.Information(msg, identity);
		}
	}
}
