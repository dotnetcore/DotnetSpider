namespace DotnetSpider.Core
{
	public interface IExecuteRecord
	{
		bool Add(string taskId, string name, string identity);
		void Remove(string taskId);
	}
}
