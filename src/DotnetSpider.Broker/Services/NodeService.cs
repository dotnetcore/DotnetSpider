using DotnetSpider.Broker.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	public class NodeService : INodeService
	{
		private readonly BrokerDbContext _dbContext;

		public NodeService(BrokerDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task AddOrUpdateNodeAsync(string connectionId, Guid nodeId, string group, string ip, int memory, string nodeType, string os, int processorCount)
		{
			var entity = await _dbContext.Node.SingleOrDefaultAsync(m => m.Id == nodeId);
			if (entity == null)
			{
				var node = new Node { ConnectionId = connectionId, Id = nodeId, CreationTime = DateTime.Now, Group = group, IpAddress = ip, IsEnabled = true, Memory = memory, NodeType = nodeType, OperatingSystem = os, ProcessorCount = processorCount };
				await _dbContext.Node.AddAsync(node);
			}
			else
			{
				entity.ConnectionId = connectionId;
				entity.Group = group;
				entity.IpAddress = ip;
				entity.Memory = memory;
				entity.NodeType = nodeType;
				entity.OperatingSystem = os;
				entity.ProcessorCount = processorCount;
				_dbContext.Node.Update(entity);
			}
			await _dbContext.SaveChangesAsync();
		}
	}
}
