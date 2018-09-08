using DotnetSpider.Broker.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	public interface INodeStatusService
	{
		Task<int> AddNodeStatusAsync(NodeStatus nodeStatus);
	}
}
