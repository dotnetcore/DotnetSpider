using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Monitor;
using MySql.Data.MySqlClient;
using Dapper;
using DotnetSpider.Core.Redial;
using NLog;
using System;

namespace DotnetSpider.Extension.Monitor
{
	public class DbMonitor : NLogMonitor
	{
		protected readonly static ILogger Logger = LogCenter.GetLogger();

		public DbMonitor(string identity)
		{
			Identity = identity;

			if (!string.IsNullOrEmpty(Config.ConnectString))
			{
				NetworkCenter.Current.Execute("dm", () =>
				{
					using (var conn = new MySqlConnection(Config.ConnectString))
					{
						InitStatusDatabase(Config.ConnectString);

						var insertSql = $"insert ignore into dotnetspider.status (`identity`, `node`, `logged`, `status`, `thread`, `left`, `success`, `error`, `total`, `avgdownloadspeed`, `avgprocessorspeed`, `avgpipelinespeed`) values (@identity, @node, current_timestamp, @status, @thread, @left, @success, @error, @total, @avgdownloadspeed, @avgprocessorspeed, @avgpipelinespeed);";
						conn.Execute(insertSql,
							new
							{
								identity = identity,
								node = NodeId.Id,
								status = "INIT",
								left = 0,
								total = 0,
								success = 0,
								error = 0,
								avgDownloadSpeed = 0,
								avgProcessorSpeed = 0,
								avgPipelineSpeed = 0,
								thread = 0
							});
					}
				});
			}
		}

		public static void InitStatusDatabase(string connectString)
		{
			using (var conn = new MySqlConnection(connectString))
			{
				try
				{
					conn.Execute("CREATE DATABASE IF NOT EXISTS `dotnetspider` DEFAULT CHARACTER SET utf8;");

					var sql = $"CREATE TABLE IF NOT EXISTS `dotnetspider`.`status` (`identity` varchar(120) NOT NULL,`node` varchar(120) NOT NULL,`logged` timestamp NULL DEFAULT current_timestamp,`status` varchar(20) DEFAULT NULL,`thread` int(13),`left` bigint(20),`success` bigint(20),`error` bigint(20),`total` bigint(20),`avgdownloadspeed` float,`avgprocessorspeed` bigint(20),`avgpipelinespeed` bigint(20), PRIMARY KEY (`identity`,`node`))";
					conn.Execute(sql);

					var trigger = conn.QueryFirstOrDefault($"SELECT TRIGGER_NAME FROM INFORMATION_SCHEMA.TRIGGERS WHERE TRIGGER_NAME = 'status_AFTER_UPDATE' and EVENT_OBJECT_SCHEMA='dotnetspider' and EVENT_OBJECT_TABLE='status'");
					if (trigger == null)
					{
						var timeTrigger = $"CREATE TRIGGER `dotnetspider`.`status_AFTER_UPDATE` BEFORE UPDATE ON `status` FOR EACH ROW BEGIN set NEW.logged = NOW(); END";
						conn.Execute(timeTrigger);
					}
				}
				catch (Exception e)
				{
					Logger.MyLog("Prepare dotnetspider.status failed.", LogLevel.Error, e);
					throw e;
				}
			}
		}

		public override void Report(string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum)
		{
			base.Report(status, left, total, success, error, avgDownloadSpeed, avgProcessorSpeed, avgPipelineSpeed, threadNum);

			if (Core.Infrastructure.Environment.SaveLogAndStatusToDb)
			{
				NetworkCenter.Current.Execute("dm", () =>
				{
					using (var conn = new MySqlConnection(Config.ConnectString))
					{
						conn.Execute(
							$"update dotnetspider.status set `status`=@status, `thread`=@thread,`left`=@left, `success`=@success, `error`=@error, `total`=@total, `avgdownloadspeed`=@avgdownloadspeed, `avgprocessorspeed`=@avgprocessorspeed, `avgpipelinespeed`=@avgpipelinespeed WHERE `identity`=@identity and `node`=@node;",
							new
							{
								identity = Identity,
								node = NodeId.Id,
								status = status,
								left = left,
								total = total,
								success = success,
								error = error,
								avgDownloadSpeed = avgDownloadSpeed,
								avgProcessorSpeed = avgProcessorSpeed,
								avgPipelineSpeed = avgPipelineSpeed,
								thread = threadNum
							});
					}
				});
			}
		}
	}
}
