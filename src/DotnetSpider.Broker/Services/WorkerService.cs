using DotnetSpider.Broker.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	public class WorkerService : IWorkerService
	{
		private readonly BrokerDbContext _dbContext;

		public WorkerService(BrokerDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<int> AddWorkerAsync(string fullClassName, string connectionId)
		{
			var worker = new Worker { FullClassName = fullClassName, ConnectionId = connectionId };
			await _dbContext.Worker.AddAsync(worker);
			await _dbContext.SaveChangesAsync();
			return worker.Id;
		}

		public async Task RemoveWorkerAsync(string fullClassName, string connectionId)
		{
			var entity = await _dbContext.Worker.SingleOrDefaultAsync(m => m.FullClassName == fullClassName && m.ConnectionId == connectionId);
			if (entity != null)
			{
				_dbContext.Worker.Remove(entity);
				await _dbContext.SaveChangesAsync();
			}
		}
	}
}
