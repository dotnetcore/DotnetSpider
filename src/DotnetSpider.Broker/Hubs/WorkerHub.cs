using DotnetSpider.Broker.Data;
using DotnetSpider.Broker.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Hubs
{
	public class WorkerHub : Hub
	{
		private readonly IWorkerService _workerService;

		public WorkerHub(IWorkerService workerService)
		{
			_workerService = workerService;
		}

		public override async Task OnConnectedAsync()
		{
			var request = Context.GetHttpContext().Request;
			var fullClassName = request.Query["fullClassName"];
			var connectionId = Context.ConnectionId;
			await _workerService.AddWorkerAsync(fullClassName, connectionId);
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			var request = Context.GetHttpContext().Request;
			var fullClassName = request.Query["fullClassName"];
			var connectionId = Context.ConnectionId;
			await _workerService.RemoveWorkerAsync(fullClassName, connectionId);
			await base.OnDisconnectedAsync(exception);
		}
	}
}
