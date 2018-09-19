using System;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	public interface INodeService
	{
		Task AddOrUpdateNodeAsync(string connectionId, Guid nodeId, string group, string ip, int memory, string nodeType, string os, int processorCount);
	}
}
