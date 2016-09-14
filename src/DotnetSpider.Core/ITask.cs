namespace DotnetSpider.Core
{
	public interface ITask
	{
		string UserId { get; }
		/// <summary>
		/// Unique id for a task.
		/// </summary>
		string Identity { get; set; }
		string TaskGroup { get; }
	}
}