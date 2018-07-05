using DotnetSpider.Core.Monitor;
using DotnetSpider.Core.Infrastructure.Database;
using System.Data;
using MySql.Data.MySqlClient;
using DotnetSpider.Core;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension.Monitor
{
	/// <summary>
	/// 数据库监控器, 把状态信息存到MySql数据库中
	/// </summary>
	public class MySqlMonitor : LogMonitor
	{
		private readonly string _connectionString;

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

					var insertSql = "insert ignore into dotnetspider.status (`taskid`,`identity`, `nodeid`, `logged`, `status`, `thread`, `left`, `success`, `error`, `total`, `avgdownloadspeed`, `avgprocessorspeed`, `avgpipelinespeed`) values (@TaskId,@Identity, @NodeId, current_timestamp, @Status, @Thread, @Left, @Success, @Error, @Total, @AvgDownloadSpeed, @AvgProcessorSpeed, @AvgPipelineSpeed);";
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
							avgprocessorspeed = 0,
							avgpipelinespeed = 0,
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
		public override void Flush(string identity, string taskId, string status, long left, long total, long success, long error, long avgDownloadSpeed, long avgProcessorSpeed, long avgPipelineSpeed, int threadNum)
		{
			if (!_isDbOnly)
			{
				base.Flush(identity, taskId, status, left, total, success, error, avgDownloadSpeed, avgProcessorSpeed, avgPipelineSpeed, threadNum);
			}

			var conn = CreateDbConnection();
			if (conn != null)
			{
				using (conn)
				{
					conn.MyExecute(
						"UPDATE dotnetspider.status SET `status`=@Status, `thread`=@Thread,`left`=@Left, `success`=@Success, `error`=@Error, `total`=@Total, `avgdownloadspeed`=@AvgDownloadSpeed, `avgprocessorspeed`=@AvgProcessorSpeed, `avgpipelinespeed`=@AvgPipelineSpeed WHERE `identity`=@Identity and `nodeid`=@NodeId;",
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
				Logger.Error("DbConnection is null.");
			}
		}

		private void InitStatusDatabase(IDbConnection conn)
		{
			try
			{
				conn.MyExecute("CREATE DATABASE IF NOT EXISTS `dotnetspider` DEFAULT CHARACTER SET utf8;");

				var sql = "CREATE TABLE IF NOT EXISTS `dotnetspider`.`status` (`taskid` varchar(120), `identity` varchar(120) NOT NULL,`nodeid` varchar(120) NOT NULL,`logged` timestamp NULL DEFAULT current_timestamp,`status` varchar(20) DEFAULT NULL,`thread` int(13),`left` bigint(20),`success` bigint(20),`error` bigint(20),`total` bigint(20),`avgdownloadspeed` float,`avgprocessorspeed` bigint(20),`avgpipelinespeed` bigint(20), PRIMARY KEY (`identity`,`nodeid`))";
				conn.MyExecute(sql);

				var trigger = conn.MyQueryFirstOrDefault("SELECT TRIGGER_NAME FROM INFORMATION_SCHEMA.TRIGGERS WHERE TRIGGER_NAME = 'status_after_update' and EVENT_OBJECT_SCHEMA='dotnetspider' and EVENT_OBJECT_TABLE='status'");
				if (trigger == null)
				{
					var timeTrigger = "CREATE TRIGGER `dotnetspider`.`status_after_update` BEFORE UPDATE ON `status` FOR EACH ROW BEGIN set NEW.logged = NOW(); END";
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
			catch
			{
				Logger.Error("Prepare DotnetSpider.Status failed.");
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
				if (Env.DataConnectionStringSettings != null && Env.DataConnectionStringSettings.ProviderName == DbProviderFactories.MySqlProvider)
				{
					conn = new MySqlConnection(Env.DataConnectionStringSettings.ConnectionString);
				}
			}
			return conn;
		}
	}
}
