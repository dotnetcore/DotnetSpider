using DotnetSpider.Common.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	public interface IBlockService
	{
		Task<BlockOutput> Pull(NodeHeartbeatInput heartbeat);
		Task Push(BlockInput input);
	}
}
