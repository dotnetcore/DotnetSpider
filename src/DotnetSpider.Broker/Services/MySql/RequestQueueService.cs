using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Services.MySql
{
	public class RequestQueueService : Services.RequestQueueService
	{
		protected RequestQueueService(BrokerOptions options, ILogger<BlockService> logger) : base(options, logger)
		{
		}

		protected override string LeftEscapeSql => "`";

		protected override string RightEscapeSql => "`";

		protected override string GetDateSql => "current_timestamp()";
	}
}
