using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Monitor;
using NLog;
using System;
using DotnetSpider.Core.Infrastructure.Database;
using System.Data;
using MySql.Data.MySqlClient;

namespace DotnetSpider.Extension.Monitor
{
	public class DbMonitor : NLogMonitor
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		private readonly bool _isDbOnly;

		public DbMonitor(string taskId, string identity, bool isDbOnly = false) : base(taskId, identity)
		{
			_isDbOnly = isDbOnly;

			if (Core.Env.SystemConnectionStringSettings != null)
			{
				using (var conn = Core.Env.SystemConnectionStringSettings.GetDbConnection())
				{
					InitStatusDatabase(conn);

					var insertSql = "insert ignore into DotnetSpider.Status (`TaskId`,`Identity`, `NodeId`, `Logged`, `Status`, `Thread`, `Left`, `Success`, `Error`, `Total`, `AvgDownloadSpeed`, `AvgProcessorSpeed`, `AvgPipelineSpeed`) values (@TaskId,@Identity, @NodeId, current_timestamp, @Status, @Thread, @Left, @Success, @Error, @Total, @AvgDownloadSpeed, @AvgProcessorSpeed, @AvgPipelineSpeed);";
					conn.MyExecute(insertSql,
						new
						{
							TaskId = _taskId,
							Identity = _identity,
							NodeId = NodeId.Id,
							Status = "INIT",
							Left = 0,
							Total = 0,
							Success = 0,
							Error = 0,
							AvgDownloadSpeed = 0,
							AvgProcessorSpeed = 0,
							AvgPipelineSpeed = 0,
							Thread = 0
						});
				}
			}
		}

		public static void InitStatusDatabase()
		{
			using (var conn = Core.Env.SystemConnectionStringSettings.GetDbConnection())
			{
				InitStatusDatabase(conn);
			}
		}

		public static void InitStatusDatabase(IDbConnection conn)
		{
			try
			{
				conn.MyExecute("CREATE DATABASE IF NOT EXISTS `DotnetSpider` DEFAULT CHARACTER SET utf8;");

				var sql = "CREATE TABLE IF NOT EXISTS `DotnetSpider`.`Status` (`TaskId` varchar(32), `Identity` varchar(120) NOT NULL,`NodeId` varchar(120) NOT NULL,`Logged` timestamp NULL DEFAULT current_timestamp,`Status` varchar(20) DEFAULT NULL,`Thread` int(13),`Left` bigint(20),`Success` bigint(20),`Error` bigint(20),`Total` bigint(20),`AvgDownloadSpeed` float,`AvgProcessorSpeed` bigint(20),`AvgPipelineSpeed` bigint(20), PRIMARY KEY (`Identity`,`NodeId`))";
				conn.MyExecute(sql);

				var trigger = conn.MyQueryFirstOrDefault("SELECT TRIGGER_NAME FROM INFORMATION_SCHEMA.TRIGGERS WHERE TRIGGER_NAME = 'Status_AFTER_UPDATE' and EVENT_OBJECT_SCHEMA='DotnetSpider' and EVENT_OBJECT_TABLE='Status'");
				if (trigger == null)
				{
					var timeTrigger = "CREATE TRIGGER `DotnetSpider`.`Status_AFTER_UPDATE` BEFORE UPDATE ON `Status` FOR EACH ROW BEGIN set NEW.Logged = NOW(); END";
					conn.MyExecute(timeTrigger);
				}
			}
			catch (MySqlException e)
			{
				if (e.Message == "This version of MySQL doesn't yet support 'multiple triggers with the same action time and event for one table'" || e.Message.Contains("Trigger already exists"))
				{
					return;
				}
				throw;
			}
			catch (Exception e)
			{
				Logger.AllLog("Prepare DotnetSpider.Status failed.", LogLevel.Error, e);
				throw;
			}
		}

		public override void Report(string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum)
		{
			if (!_isDbOnly)
			{
				base.Report(status, left, total, success, error, avgDownloadSpeed, avgProcessorSpeed, avgPipelineSpeed, threadNum);
			}

			using (var conn = Core.Env.SystemConnectionStringSettings.GetDbConnection())
			{
				conn.MyExecute(
					"update DotnetSpider.Status SET `Status`=@Status, `Thread`=@Thread,`Left`=@Left, `Success`=@Success, `Error`=@Error, `Total`=@Total, `AvgDownloadSpeed`=@AvgDownloadSpeed, `AvgProcessorSpeed`=@AvgProcessorSpeed, `AvgPipelineSpeed`=@AvgPipelineSpeed WHERE `Identity`=@Identity and `NodeId`=@NodeId;",
					new
					{
						Identity = _identity,
						NodeId = NodeId.Id,
						Status = status,
						Left = left,
						Total = total,
						Success = success,
						Error = error,
						AvgDownloadSpeed = avgDownloadSpeed,
						AvgProcessorSpeed = avgProcessorSpeed,
						AvgPipelineSpeed = avgPipelineSpeed,
						Thread = threadNum
					});
			}
		}
	}
}
