using DotnetSpider.Broker.Data;
using DotnetSpider.Broker.Hubs;
using DotnetSpider.Broker.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DotnetSpider.Broker.Test
{
	public class NodeHubTest
	{
		[Fact]
		public void HubsAreMockableViaDynamic()
		{
			var brokerDbContext = new Mock<BrokerDbContext>();
			var nodes = new List<Node>();
			var nodeStatues = new List<NodeStatus>();
			brokerDbContext.Setup(x => x.Node).ReturnsDbSet(nodes);
			brokerDbContext.Setup(x => x.NodeStatus).ReturnsDbSet(nodeStatues);

			var context = new Mock<HubCallerContext>();
			var hub = new NodeHub(new NodeService(brokerDbContext.Object), new NodeStatusService(brokerDbContext.Object));
			hub.Context = context.Object;
			hub.OnConnectedAsync().Wait();
		}
	}
}
