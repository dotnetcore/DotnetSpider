using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Infrastructure.Database;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension
{
	public class MySqlExecuteRecord : IExecuteRecord
	{
		private static readonly ILogger Logger = LogCenter.GetLogger();

		public ISpider Spider { get; private set; }

		public bool Add()
		{
			if (Env.SystemConnectionStringSettings != null && !string.IsNullOrEmpty(Spider.TaskId))
			{
				using (IDbConnection conn = Env.SystemConnectionStringSettings.GetDbConnection())
				{
					conn.MyExecute("CREATE SCHEMA IF NOT EXISTS `DotnetSpider` DEFAULT CHARACTER SET utf8mb4;");
					conn.MyExecute("CREATE TABLE IF NOT EXISTS `DotnetSpider`.`TaskRunning` (`__Id` bigint(20) NOT NULL AUTO_INCREMENT, `TaskId` varchar(120) NOT NULL, `Name` varchar(200) NULL, `Identity` varchar(120), `CDate` timestamp NOT NULL DEFAULT current_timestamp, PRIMARY KEY (__Id), UNIQUE KEY `taskId_unique` (`TaskId`)) AUTO_INCREMENT=1");
					conn.MyExecute($"INSERT IGNORE INTO `DotnetSpider`.`TaskRunning` (`TaskId`,`Name`,`Identity`) values ('{Spider.TaskId}','{Spider.Name}','{Spider.Identity}');");
				}
			}
			return true;
		}

		public MySqlExecuteRecord(ISpider spider)
		{
			Spider = spider;
		}

		public void Remove()
		{
			if (Env.SystemConnectionStringSettings != null && !string.IsNullOrEmpty(Spider.TaskId))
			{
				using (IDbConnection conn = Env.SystemConnectionStringSettings.GetDbConnection())
				{
					conn.MyExecute($"DELETE FROM `DotnetSpider`.`TaskRunning` WHERE `Identity`='{Spider.Identity}';");
				}
			}
		}
	}
}
