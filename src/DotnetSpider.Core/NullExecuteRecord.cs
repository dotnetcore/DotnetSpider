using Microsoft.Extensions.Logging;

namespace DotnetSpider.Core
{
	public class NullExecuteRecord : IExecuteRecord
	{
		public ILogger Logger { get; set; }

		public bool Add(string taskId, string name, string identity)
		{
			return true;
		}

		public void Remove(string taskId, string name, string identity)
		{
		}
	}
}
