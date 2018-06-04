using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Infrastructure;
using System.Configuration;
using System.Data;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Pipeline;
using System.Collections.Generic;
using System;
using System.Linq;
using Serilog;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class DbModelPipeline : ModelPipeline
	{
		public static string[] RetryExceptionMessages = new[] { "Unable to connect", "Access denied for user" };

		private PipelineMode _pipelineMode;
		private readonly Dictionary<string, Sqls> _sqls = new Dictionary<string, Sqls>();

		public int RetryTimes { get; set; } = 600;

		public string ConnectString { get; private set; }

		/// <summary>
		/// 数据库忽略大小写
		/// </summary>
		public bool IgnoreColumnCase { get; set; } = true;

		/// <summary>
		/// 自动添加时间戳
		/// </summary>
		public bool AutoTimestamp { get; set; } = true;

		/// <summary>
		/// 更新数据库连接字符串的接口, 如果自定义的连接字符串无法使用, 则会尝试通过更新连接字符串来重新连接
		/// </summary>
		public IConnectionStringSettingsRefresher ConnectionStringSettingsRefresher { get; set; }

		protected DbModelPipeline(string connectString = null, PipelineMode pipelineMode = PipelineMode.InsertAndIgnoreDuplicate)
		{
			ConnectString = connectString;
			PipelineMode = pipelineMode;
		}

		protected abstract IDbConnection CreateDbConnection(string connectString);

		protected abstract Sqls GenerateSqls(IModel model);

		protected abstract void InitDatabaseAndTable(IDbConnection conn, IModel model);

		protected override int Process(IModel model, IEnumerable<dynamic> datas, ISpider spider)
		{
			if (model.TableInfo == null)
			{
				return 0;
			}

			IDbConnection conn = null;

			for (int i = 0; i < RetryTimes; ++i)
			{
				try
				{
					conn = RefreshConnectionString();

					Sqls sqls;
					lock (this)
					{
						if (_sqls.ContainsKey(model.Identity))
						{
							sqls = _sqls[model.Identity];
						}
						else
						{
							sqls = GenerateSqls(model);
							_sqls.Add(model.Identity, sqls);
							InitDatabaseAndTable(conn, model);
						}
					}

					int count = 0;

					switch (_pipelineMode)
					{
						case PipelineMode.Insert:
							{
								count += conn.MyExecute(sqls.InsertSql, datas);
								break;
							}
						case PipelineMode.InsertAndIgnoreDuplicate:
							{
								count += conn.MyExecute(sqls.InsertAndIgnoreDuplicateSql, datas);
								break;
							}
						case PipelineMode.InsertNewAndUpdateOld:
							{
								count += conn.MyExecute(sqls.InsertNewAndUpdateOldSql, datas);
								break;
							}
						case PipelineMode.Update:
							{
								if (string.IsNullOrWhiteSpace(sqls.UpdateSql))
								{
									Log.Logger.Error("Check your TableInfo attribute contains UpdateColumns value.");
									throw new SpiderException("UpdateSql is null.");
								}
								count += conn.MyExecute(sqls.UpdateSql, datas);
								break;
							}
						default:
							{
								count += conn.MyExecute(sqls.InsertSql, datas);
								break;
							}
					}

					return count;
				}
				catch (Exception e)
				{
					if (RetryExceptionMessages.Any(m => e.Message.Contains(m)))
					{
						Thread.Sleep(5000);
						continue;
					}
					throw e;
				}
				finally
				{
					conn?.Dispose();
				}
			}
			throw new SpiderException($"Pipeline process failed.");
		}

		private IDbConnection RefreshConnectionString()
		{
			if (!string.IsNullOrWhiteSpace(ConnectString))
			{
				var conn = TryCreateDbConnection();
				if (conn != null)
				{
					return conn;
				}
			}
			if (!string.IsNullOrWhiteSpace(Env.DataConnectionString))
			{
				ConnectString = Env.DataConnectionString;
				var conn = TryCreateDbConnection();
				if (conn != null)
				{
					return conn;
				}
			}
			if (ConnectionStringSettingsRefresher != null)
			{
				ConnectString = ConnectionStringSettingsRefresher.GetNew().ConnectionString;
				var conn = TryCreateDbConnection();
				if (conn != null)
				{
					return conn;
				}
			}

			throw new SpiderException("Can't find connectstring from argument, configfile, connectionStringSettingsRefresher.");
		}

		/// <summary>
		/// 默认的数据管道模式
		/// </summary>
		public PipelineMode PipelineMode
		{
			get => _pipelineMode;
			set
			{
				if (!Equals(value, _pipelineMode))
				{
					_pipelineMode = value;
				}
			}
		}

		/// <summary>
		/// 从配置文件中获取默认的数据管道
		/// </summary>
		/// <returns>数据管道</returns>
		public static IPipeline GetPipelineFromAppConfig(ConnectionStringSettings connectionStringSettings = null)
		{
			connectionStringSettings = connectionStringSettings == null ? Env.DataConnectionStringSettings : connectionStringSettings;
			if (connectionStringSettings == null)
			{
				return null;
			}
			IPipeline pipeline = new ConsolePipeline();
			switch (connectionStringSettings.ProviderName)
			{
				case DbProviderFactories.PostgreSqlProvider:
					{
						pipeline = new PostgreSqlEntityPipeline(connectionStringSettings.ConnectionString);
						break;
					}
				case DbProviderFactories.MySqlProvider:
					{
						pipeline = new MySqlEntityPipeline(connectionStringSettings.ConnectionString);
						break;
					}
				case DbProviderFactories.SqlServerProvider:
					{
						pipeline = new SqlServerEntityPipeline(connectionStringSettings.ConnectionString);
						break;
					}
				case "MongoDB":
					{
						pipeline = new MongoDbEntityPipeline(connectionStringSettings.ConnectionString);
						break;
					}
				//case "Cassandra":
				//	{
				//		pipeline = new CassandraEntityPipeline(connectionStringSettings.ConnectionString);
				//		break;
				//	}
				//case "HttpMySql":
				//	{
				//		pipeline = new HttpMySqlEntityPipeline();
				//		break;
				//	}
				default:
					{
						pipeline = new ConsolePipeline();
						break;
					}
			}
			return pipeline;
		}

		private IDbConnection TryCreateDbConnection()
		{
			try
			{
				var conn = CreateDbConnection(ConnectString);
				conn.Open();
				return conn;
			}
			catch
			{
				Log.Logger.Warning($"Can't connect to database: {ConnectString}.");
			}
			return null;
		}
	}
}

