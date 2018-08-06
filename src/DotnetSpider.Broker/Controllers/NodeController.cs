using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Broker.Services;
using DotnetSpider.Common;
using DotnetSpider.Common.Dto;
using LZ4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotnetSpider.Broker.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NodeController : BrokerController
	{
		private readonly INodeService _nodeService;
		private readonly IBlockService _blockService;

		public NodeController(ILogger<NodeController> logger, BrokerOptions options, INodeService nodeService, IBlockService blockService) : base(logger, options)
		{
			_nodeService = nodeService;
			_blockService = blockService;
		}

		[HttpDelete("Unregister")]
		public async Task Unregister(string nodeId)
		{
			_logger.LogInformation($"{GetRemoveIpAddress()} disconnected.");
			await _nodeService.Remove(nodeId);
		}

		[HttpPost("Heartbeat")]
		public async Task<IActionResult> Heartbeat([FromBody]NodeHeartbeatInput heartbeat)
		{
			IActionResult result = BadRequest();
			if (ModelState.IsValid)
			{
				await _nodeService.Heartbeat(heartbeat);
				var output = await _blockService.Pop(heartbeat);
				result = new JsonResult(output);
			}

			return result;
		}

		[HttpPost("Block")]
		public async Task<IActionResult> Block()
		{
			var bytes = HttpContext.Request.Body.ToBytes();
			var json = Encoding.UTF8.GetString(LZ4Codec.Unwrap(bytes));
			var input = JsonConvert.DeserializeObject<BlockInput>(json);
			IActionResult result = BadRequest();
			if (TryValidateModel(input))
			{
				await _blockService.Callback(input);
				result = Ok();
			}

			return result;
		}
	}
}
