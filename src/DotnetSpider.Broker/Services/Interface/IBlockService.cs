using DotnetSpider.Common.Dto;
using DotnetSpider.Common.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services
{
	public interface IBlockService
	{
		Task<BlockOutput> Pop(NodeHeartbeatInput heartbeat);
		Task Callback(BlockInput input);
		Task Add(Block block);
		Task Update(Block block);
		Task<Block> GetOneCompletedByIdentity(string identity);
	}
}
