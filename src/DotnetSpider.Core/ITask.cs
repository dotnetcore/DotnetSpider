namespace DotnetSpider.Core
{
	public interface ITask
	{
		string UserId { get; }

		string TaskGroup { get; }
	}
}