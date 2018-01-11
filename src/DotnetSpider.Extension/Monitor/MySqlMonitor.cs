using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Monitor;
using System;
using DotnetSpider.Core.Infrastructure.Database;
using System.Data;
using MySql.Data.MySqlClient;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Monitor
{
	/// <summary>
	/// 数据库监控器, 把状态信息存到MySql数据库中
	/// </summary>
	public class MySqlMonitor : NLogMonitor
	{
		private readonly string _connectionString;

		/// <summary>
		/// 日志接口
		/// </summary>
		protected static readonly ILogger Logger = DLog.GetLogger();

		private readonly bool _isDbOnly;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="taskId">任务编号</param>
		/// <param name="identity">任务标识</param>
		/// <param name="isDbOnly">是否仅上报数据库</param>
		/// <param name="connectionString">连接字符串</param>
		public MySqlMonitor(string taskId, string identity, bool isDbOnly = false, string connectionString = null)
		{
			_connectionString = connectionString;
			_isDbOnly = isDbOnly;

			var conn = CreateDbConnection();
			if (conn != null)
			{
				using (conn)
				{
					InitStatusDatabase(conn);

					var insertSql = "insert ignore into DotnetSpider.Status (`TaskId`,`Identity`, `NodeId`, `Logged`, `Status`, `Thread`, `Left`, `Success`, `Error`, `Total`, `AvgDownloadSpeed`, `AvgProcessorSpeed`, `AvgPipelineSpeed`) values (@TaskId,@Identity, @NodeId, current_timestamp, @Status, @Thread, @Left, @Success, @Error, @Total, @AvgDownloadSpeed, @AvgProcessorSpeed, @AvgPipelineSpeed);";
					conn.MyExecute(insertSql,
						new
						{
							TaskId = taskId,
							Identity = identity,
							Env.NodeId,
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

		/// <summary>
		/// 上报爬虫状态
		/// </summary>
		/// <param name="identity">唯一标识</param>
		/// <param name="taskId">任务编号</param>
		/// <param name="status">爬虫状态: 运行、暂停、退出、完成</param>
		/// <param name="left">剩余的目标链接数</param>
		/// <param name="total">总的需要采集的链接数</param>
		/// <param name="success">成功采集的链接数</param>
		/// <param name="error">采集出错的链接数</param>
		/// <param name="avgDownloadSpeed">平均下载一个链接需要的时间(豪秒)</param>
		/// <param name="avgProcessorSpeed">平均解析一个页面需要的时间(豪秒)</param>
		/// <param name="avgPipelineSpeed">数据管道处理一次数据结果需要的时间(豪秒)</param>
		/// <param name="threadNum">爬虫线程数</param>
		public override void Report(string identity, string taskId, string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum)
		{
			if (!_isDbOnly)
			{
				base.Report(identity, taskId, status, left, total, success, error, avgDownloadSpeed, avgProcessorSpeed, avgPipelineSpeed, threadNum);
			}

			var conn = CreateDbConnection();
			if (conn != null)
			{
				using (conn)
				{
					Logger.NLog(identity, "Report status.", Level.Error);
					conn.MyExecute(
						"update DotnetSpider.Status SET `Status`=@Status, `Thread`=@Thread,`Left`=@Left, `Success`=@Success, `Error`=@Error, `Total`=@Total, `AvgDownloadSpeed`=@AvgDownloadSpeed, `AvgProcessorSpeed`=@AvgProcessorSpeed, `AvgPipelineSpeed`=@AvgPipelineSpeed WHERE `Identity`=@Identity and `NodeId`=@NodeId;",
						new
						{
							Identity = identity,
							Env.NodeId,
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
			else
			{
				Logger.NLog(identity, "DbConnection is null.", Level.Error);
			}
		}

		private void InitStatusDatabase(IDbConnection conn)
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
				Logger.Log("Prepare DotnetSpider.Status failed.", Level.Error, e);
				throw;
			}
		}

		private IDbConnection CreateDbConnection()
		{
			IDbConnection conn = null;
			if (!string.IsNullOrWhiteSpace(_connectionString))
			{
				conn = new MySqlConnection(_connectionString);
			}
			else
			{
				if (Env.SystemConnectionStringSettings != null && Env.SystemConnectionStringSettings.ProviderName == DbProviderFactories.MySqlProvider)
				{
					conn = new MySqlConnection(Env.SystemConnectionStringSettings.ConnectionString);
				}
			}
			return conn;
		}
	}
}
