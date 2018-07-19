using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure.Database;
using System;
using System.Data;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Common;

namespace DotnetSpider.Extension
{
	/// <summary>
	/// MySql 运行记录接口
	/// 程序在运行前应该添加相应的运行记录, 任务结束后删除对应的记录, 企业服务依赖运行记录数据显示正在运行的任务
	/// </summary>
	public class MySqlExecuteRecord : IExecuteRecord
	{
		public ILogger Logger { get; set; }

		public MySqlExecuteRecord(ILogger logger)
		{
			Logger = logger;
		}

		/// <summary>
		/// 添加运行记录
		/// </summary>
		/// <param name="taskId">任务编号</param>
		/// <param name="name">任务名称</param>
		/// <param name="identity">任务标识</param>
		/// <returns>是否添加成功</returns>
		public bool Add(string taskId, string name, string identity)
		{
			try
			{
				if (Env.DataConnectionStringSettings != null && !string.IsNullOrWhiteSpace(taskId) && !string.IsNullOrWhiteSpace(identity))
				{
					using (IDbConnection conn = Env.DataConnectionStringSettings.CreateDbConnection())
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
				Logger.Error($"Add execute record failed: {e}", identity);
				return false;
			}
		}

		/// <summary>
		/// 删除运行记录
		/// </summary>
		/// <param name="taskId">任务编号</param>
		/// <param name="name">任务名称</param>
		/// <param name="identity">任务标识</param>
		public void Remove(string taskId, string name, string identity)
		{
			try
			{
				if (Env.DataConnectionStringSettings != null && !string.IsNullOrWhiteSpace(taskId))
				{
					using (IDbConnection conn = Env.DataConnectionStringSettings.CreateDbConnection())
					{
						conn.MyExecute($"DELETE FROM `dotnetspider`.`taskrunning` WHERE `identity`='{taskId}';");
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error($"Remove execute record failed: {e}");
			}
		}
	}
}
