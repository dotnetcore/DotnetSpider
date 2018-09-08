using DotnetSpider.Broker.Data;
using DotnetSpider.Broker.Hubs;
using DotnetSpider.Broker.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DotnetSpider.Broker.Test
{
	public class WorkerServiceTest : BaseTest
	{
		private readonly IWorkerService _workerService;
		private readonly BrokerDbContext _dbContext;

		public WorkerServiceTest() : base()
		{
			_workerService = Services.GetRequiredService<IWorkerService>();
			_dbContext = Services.GetRequiredService<BrokerDbContext>();
		}

		[Fact(DisplayName = "AddWorker")]
		public async void AddWorker()
		{
			var fullClassName = Guid.NewGuid().ToString();
			var connectionId = Guid.NewGuid().ToString();
			var id = await _workerService.AddWorkerAsync(fullClassName, connectionId);
			var worker = _dbContext.Worker.Find(id);
			Assert.Equal(fullClassName, worker.FullClassName);
			Assert.Equal(connectionId, worker.ConnectionId);
		}

		[Fact(DisplayName = "RemoveWorker")]
		public async void RemoveWorker()
		{
			var fullClassName = Guid.NewGuid().ToString();
			var connectionId = Guid.NewGuid().ToString();
			var id = await _workerService.AddWorkerAsync(fullClassName, connectionId);
			var worker = _dbContext.Worker.Find(id);
			Assert.NotNull(worker);

			await _workerService.RemoveWorkerAsync(fullClassName, connectionId);
			worker = _dbContext.Worker.Find(id);
			Assert.Null(worker);
		}
	}
}
