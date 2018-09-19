using System.Threading.Tasks;
using DotnetSpider.Broker.Data;

namespace DotnetSpider.Broker.Services
{
	public class NodeStatusService : INodeStatusService
	{
		private readonly BrokerDbContext _dbContext;

		public NodeStatusService(BrokerDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<int> AddNodeStatusAsync(NodeStatus nodeStatus)
		{
			await _dbContext.NodeStatus.AddAsync(nodeStatus);
			await _dbContext.SaveChangesAsync();
			return nodeStatus.Id;
		}
	}
}
