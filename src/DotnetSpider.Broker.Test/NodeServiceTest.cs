using DotnetSpider.Broker.Services;
using DotnetSpider.Common.Dto;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DotnetSpider.Broker.Test
{
	public class NodeServiceTest : BaseTest
	{
		public NodeServiceTest()
		{
			var options = new BrokerOptions
			{
				ConnectionString = "Server=.\\SQLEXPRESS;Database=DotnetSpider_Dev;Integrated Security = SSPI;",
				StorageType = StorageType.SqlServer,
				Tokens = new HashSet<string> { "aaa" },
				UseToken = false
			};
			Init(options);
		}

		[Fact(DisplayName = "Heartbeat")]
		public async Task<NodeHeartbeatInput> Heartbeat()
		{
			var service = Services.GetRequiredService<INodeService>();
			var id = Guid.NewGuid().ToString("N");
			var cpuCount = 1000;
			var cpu = 1000;
			var freeMemory = 1000;
			var group = "TESTGROUP";
			var ip = "IPADDR1";
			var os = "MYOS";
			var totalMemory = 1000;
			var heartbeat = new NodeHeartbeatInput
			{
				Cpu = cpu,
				CpuCount = cpuCount,
				FreeMemory = freeMemory,
				Group = group,
				Ip = ip,
				NodeId = id,
				Os = os,
				TotalMemory = totalMemory,
				Runnings = new string[] { "p1", "p2" }
			};
			await service.Heartbeat(heartbeat);
			var node = await service.Get(id);
			var lastHeartbeat = await service.GetLastHeartbeat(id);
			Assert.Equal(id, node.NodeId);
			Assert.Equal(cpuCount, node.CpuCount);
			Assert.Equal(group, node.Group);
			Assert.Equal(ip, node.Ip);
			Assert.True(node.IsEnable);
			Assert.Equal(os, node.Os);
			Assert.Equal(totalMemory, node.TotalMemory);

			Assert.Equal(cpu, heartbeat.Cpu);
			Assert.Equal(freeMemory, heartbeat.FreeMemory);
			Assert.Equal(2, lastHeartbeat.ProcessCount);
			return heartbeat;
		}

		[Fact(DisplayName = "RemoveNode")]
		public async void RemoveNode()
		{
			var heartbeat = await Heartbeat();
			var service = Services.GetRequiredService<INodeService>();
			await service.Remove(heartbeat.NodeId);
			var node = await service.Get(heartbeat.NodeId);
			Assert.Null(node);
		}

		[Fact(DisplayName = "UpdateNode")]
		public async void UpdateNode()
		{
			var heartbeat = await Heartbeat();
			var service = Services.GetRequiredService<INodeService>();
			var node = await service.Get(heartbeat.NodeId);
			var cpuCount = 2000;
			var group = "TESTGROUP2";
			var ip = "IPADDR2";
			var os = "MYOS2";
			var totalMemory = 2000;
			node.Group = group;
			node.CpuCount = cpuCount;
			node.Ip = ip;
			node.Os = os;
			node.TotalMemory = totalMemory;
			node.IsEnable = false;

			await service.AddOrUpdate(node);
			var newNode = await service.Get(heartbeat.NodeId);

			Assert.Equal(cpuCount, node.CpuCount);
			Assert.Equal(group, node.Group);
			Assert.Equal(ip, node.Ip);
			Assert.False(node.IsEnable);
			Assert.Equal(os, node.Os);
			Assert.Equal(totalMemory, node.TotalMemory);
		}
	}
}
