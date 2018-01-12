using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Infrastructure.Database;
using System;
using System.Data;

namespace DotnetSpider.Extension
{
	public class MySqlExecuteRecord : IExecuteRecord
	{
		private static readonly ILogger Logger = DLog.GetLogger();

		public bool Add(string taskId, string name, string identity)
		{
			try
			{
				if (Env.SystemConnectionStringSettings != null && !string.IsNullOrWhiteSpace(taskId) && !string.IsNullOrWhiteSpace(identity))
				{
					using (IDbConnection conn = Env.SystemConnectionStringSettings.CreateDbConnection())
					{
						conn.MyExecute("CREATE SCHEMA IF NOT EXISTS `dotnetspider` DEFAULT CHARACTER SET utf8;");
						conn.MyExecute("CREATE TABLE IF NOT EXISTS `dotnetspider`.`taskrunning` (`__id` bigint(20) NOT NULL AUTO_INCREMENT, `taskid` varchar(120) NOT NULL, `name` varchar(200) NULL, `identity` varchar(120), `cdate` timestamp NOT NULL DEFAULT current_timestamp, PRIMARY KEY (__id), UNIQUE KEY `taskid_unique` (`taskid`)) AUTO_INCREMENT=1");
						conn.MyExecute($"INSERT IGNORE INTO `dotnetspider`.`taskrunning` (`taskid`,`name`,`identity`) values ('{taskId}','{name}','{identity}');");
					}
				}
				return true;
			}
			catch (Exception e)
			{
				Logger.NLog($"Add execute record failed: {e}", Level.Error);
				return false;
			}
		}

		public void Remove(string taskId, string name, string identity)
		{
			try
			{
				if (Env.SystemConnectionStringSettings != null && !string.IsNullOrWhiteSpace(taskId))
				{
					using (IDbConnection conn = Env.SystemConnectionStringSettings.CreateDbConnection())
					{
						conn.MyExecute($"DELETE FROM `dotnetspider`.`taskrunning` WHERE `identity`='{taskId}';");
					}
				}
			}
			catch (Exception e)
			{
				Logger.NLog($"Remove execute record failed: {e}", Level.Error);
			}
		}
	}
}
