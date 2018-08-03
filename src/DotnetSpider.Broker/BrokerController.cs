using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker
{
	public abstract class BrokerController : ControllerBase
	{
		protected readonly ILogger _logger;
		protected readonly BrokerOptions _options;

		public BrokerController(ILogger<BrokerController> logger, BrokerOptions options)
		{
			_logger = logger;
			_options = options;
		}

		protected string GetRemoveIpAddress()
		{
			return HttpContext.Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
		}
	}
}
