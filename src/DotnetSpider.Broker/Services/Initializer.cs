using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Common.Entity;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Broker.Services.MySql
{
	public class Initializer : BaseService, IInitializer
	{
		private readonly IRunningService _runningService;
		private readonly IRunningHistoryService _runningHistoryService;

		public Initializer(BrokerOptions options, IRunningService runningService, IRunningHistoryService runningHistoryService, ILogger<Initializer> logger) : base(options, logger)
		{
			_runningService = runningService;
			_runningHistoryService = runningHistoryService;
		}

		public void Init()
		{
			using (var conn = CreateDbConnection())
			{
				if (_runningService.GetAll().Result.Count == 0)
				{
					var id = Guid.NewGuid().ToString("N");
					_runningService.Add(new Running { Identity = id, BlockTimes = 0, CreationTime = DateTime.Now, Priority = 0, LastModificationTime = DateTime.Now });
					_runningHistoryService.Add(new RunningHistory { Identity = id, CreationTime = DateTime.Now, LastModificationTime = DateTime.Now });
				}
			}
		}

		//		private void CreateDatabase(IDbConnection conn)
		//		{
		//			conn.Execute("CREATE DATABASE IF NOT EXISTS `dotnetspider` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
		//		}

		//		private void CreateNodeTable(IDbConnection conn)
		//		{
		//			conn.Execute(@"
		//CREATE TABLE IF NOT EXISTS dotnetspider.node (
		//  node_id varchar(32) NOT NULL,
		//  ip varchar(32) NOT NULL,
		//  cpu_count int NOT NULL,
		//  is_enable tinyint(1) NOT NULL,
		//  `group` varchar(32) NOT NULL,
		//  os varchar(32) NOT NULL,
		//  totalMemory int NOT NULL,
		//  creation_time timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
		//  last_modification_time timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
		//  PRIMARY KEY(node_id)
		//) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;
		//");
		//		}

		//		private void CreateNodeHeartbeatTable(IDbConnection conn)
		//		{
		//			conn.Execute(@"
		//CREATE TABLE IF NOT EXISTS dotnetspider.node_heartbeat (
		//  id bigint(20) NOT NULL AUTO_INCREMENT,
		//  node_id varchar(32) NOT NULL,
		//  cpu int NOT NULL,
		//  free_memory int NOT NULL,
		//  creationtime timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
		//  PRIMARY KEY(id)
		//) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;
		//");
		//		}
	}
}
