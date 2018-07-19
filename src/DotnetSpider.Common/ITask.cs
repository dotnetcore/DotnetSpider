namespace DotnetSpider.Common
{
	/// <summary>
	/// 任务接口, 配合企业管理平台使用
	/// </summary>
	public interface ITask
	{
		/// <summary>
		/// 任务编号, 单独使用时非必须, 配合企业管理平台使用
		/// </summary>
		string TaskId { get; set; }
	}
}
