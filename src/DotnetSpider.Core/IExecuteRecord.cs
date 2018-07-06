namespace DotnetSpider.Core
{
	/// <summary>
	/// 运行记录接口
	/// 程序在运行前应该添加相应的运行记录, 任务结束后删除对应的记录, 企业服务依赖运行记录数据显示正在运行的任务
	/// </summary>
	public interface IExecuteRecord
	{
		/// <summary>
		/// 添加运行记录
		/// </summary>
		/// <param name="taskId">任务编号</param>
		/// <param name="name">任务名称</param>
		/// <param name="identity">任务标识</param>
		/// <returns>是否添加成功</returns>
		bool Add(string taskId, string name, string identity);

		/// <summary>
		/// 删除运行记录
		/// </summary>
		/// <param name="taskId">任务编号</param>
		/// <param name="name">任务名称</param>
		/// <param name="identity">任务标识</param>
		void Remove(string taskId, string name, string identity);
	}
}
