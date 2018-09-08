using DotnetSpider.Broker.Data;
using DotnetSpider.Broker.Dtos;
using DotnetSpider.Broker.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Hubs
{
	public class NodeHub : Hub
	{
		private readonly INodeService _nodeService;
		private readonly INodeStatusService _nodeStatusService;

		public NodeHub(INodeService nodeService, INodeStatusService nodeStatusService)
		{
			_nodeService = nodeService;
			_nodeStatusService = nodeStatusService;
		}

		public override async Task OnConnectedAsync()
		{
			var request = Context.GetHttpContext().Request;
			var nodeId = request.Query["nodeId"];
			var group = request.Query["group"];
			var ip = request.Query["ip"];
			var memory = int.Parse(request.Query["memory"]);
			var nodeType = request.Query["nodeType"];
			var os = request.Query["os"];
			var processorCount = int.Parse(request.Query["processorCount"]);
			var connectionId = Context.ConnectionId;
			await _nodeService.AddOrUpdateNodeAsync(connectionId, Guid.Parse(nodeId), group, ip, memory, nodeType, os, processorCount);
			await base.OnConnectedAsync();
		}

		public async Task Heartbeat(AddNodeStatusDto input)
		{
			var request = Context.GetHttpContext().Request;
			var nodeId = request.Query["nodeId"];
			var nodeStatus = new NodeStatus { Cpu = input.Cpu, CreationTime = DateTime.Now, FreeMemory = input.FreeMemory, NodeId = Guid.Parse(nodeId), ProcessCount = input.ProcessCount };
			await _nodeStatusService.AddNodeStatusAsync(nodeStatus);
		}
	}
}
