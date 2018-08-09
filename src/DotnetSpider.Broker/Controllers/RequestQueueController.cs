using DotnetSpider.Broker.Services;
using DotnetSpider.Common;
using DotnetSpider.Common.Entity;
using LZ4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("DotnetSpider.Broker.Test")]
namespace DotnetSpider.Broker.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RequestQueueController : BrokerController
	{
		private readonly IRequestQueueService _requestQueueService;
		private readonly IBlockService _blockService;

		public RequestQueueController(ILogger<BrokerController> logger,
			BrokerOptions options,
			IRequestQueueService requestQueueService,
			IBlockService blockService) : base(logger, options)
		{
			_requestQueueService = requestQueueService;
			_blockService = blockService;
		}

		[HttpPost]
		public async Task<IActionResult> Enqueue(string identity)
		{
			var bytes = HttpContext.Request.Body.ToBytes();
			var json = Encoding.UTF8.GetString(LZ4Codec.Unwrap(bytes));
			await _requestQueueService.Add(identity, json);
			return Ok();
		}

		public async Task<IActionResult> Dequeue(string identity)
		{
			Block block = await _blockService.GetOneCompletedByIdentity(identity);
			var requestQueues = await _requestQueueService.GetByBlockId(block.BlockId);
			var requests = requestQueues.Select(r =>
			{
				  return JsonConvert.DeserializeObject<Request>(r.Request);
			}).ToList();
			return new JsonResult(requests);
		}
	}
}
