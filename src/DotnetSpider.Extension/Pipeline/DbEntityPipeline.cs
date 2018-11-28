using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Extension.Infrastructure;
using System.Configuration;
using System.Data;
using DotnetSpider.Core.Pipeline;
using System.Collections.Generic;
using System;
using System.Linq;
using DotnetSpider.Extraction.Model;
using Microsoft.Extensions.Logging;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class DbEntityPipeline : EntityPipeline
	{
		public static readonly HashSet<string> RetryExceptionMessages = new HashSet<string>
			{"Unable to connect", "Access denied for user"};

		private PipelineMode _pipelineMode;
		private readonly Dictionary<string, SqlStatements> _sqlStatements = new Dictionary<string, SqlStatements>();

		public int RetryTimes { get; set; } = 600;

		public string ConnectString { get; set; }

		/// <summary>
		/// 是否使用事务操作。默认不使用。
		/// </summary>
		public bool UseTransaction { get; set; } = false;

		/// <summary>
		/// 数据库忽略大小写
		/// </summary>
		public bool IgnoreCase { get; set; } = true;

		/// <summary>
		/// 自动添加时间戳
		/// </summary>
		public bool AutoTimestamp { get; set; } = true;

		/// <summary>
		/// 更新数据库连接字符串的接口, 如果自定义的连接字符串无法使用, 则会尝试通过更新连接字符串来重新连接
		/// </summary>
		public IConnectionStringSettingsRefresher ConnectionStringSettingsRefresher { get; set; }

		protected DbEntityPipeline(string connectString = null,
			PipelineMode pipelineMode = PipelineMode.InsertAndIgnoreDuplicate)
		{
			ConnectString = connectString;
			PipelineMode = pipelineMode;
		}

		protected abstract IDbConnection CreateDbConnection(string connectString);

		protected abstract SqlStatements GenerateSqlStatements(TableInfo model);

		protected abstract void InitDatabaseAndTable(IDbConnection conn, TableInfo model);

		protected override int Process(List<IBaseEntity> items, dynamic sender = null)
		{
			if (items == null || items.Count == 0)
			{
				return 0;
			}

			var tableInfo = new TableInfo(items.First().GetType());

			IDbConnection conn = null;

			for (int i = 0; i < RetryTimes; ++i)
			{
				try
				{
					conn = RefreshConnectionString();

					// 每天执行一次建表操作, 可以实现每天一个表的操作，或者按周分表可以在运行时创建新表。
					var key = tableInfo.Schema.TableNamePostfix != TableNamePostfix.None
						? $"{tableInfo.Schema.TableName}_{DateTime.Now:yyyyMMdd}"
						: tableInfo.Schema.TableName;
					SqlStatements sqlStatements;
					lock (this)
					{
						if (_sqlStatements.ContainsKey(key))
						{
							sqlStatements = _sqlStatements[key];
						}
						else
						{
							sqlStatements = GenerateSqlStatements(tableInfo);
							_sqlStatements.Add(key, sqlStatements);
							InitDatabaseAndTable(conn, tableInfo);
						}
					}

					int count = 0;
					IDbTransaction transaction = null;
					if (UseTransaction)
					{
						transaction = conn.BeginTransaction();
					}

					switch (_pipelineMode)
					{
						case PipelineMode.Insert:
						{
							count += conn.MyExecute(sqlStatements.InsertSql, items, transaction);
							break;
						}
						case PipelineMode.InsertAndIgnoreDuplicate:
						{
							count += conn.MyExecute(sqlStatements.InsertAndIgnoreDuplicateSql, items, transaction);
							break;
						}
						case PipelineMode.InsertNewAndUpdateOld:
						{
							count += conn.MyExecute(sqlStatements.InsertNewAndUpdateOldSql, items, transaction);
							break;
						}
						case PipelineMode.Update:
						{
							if (string.IsNullOrWhiteSpace(sqlStatements.UpdateSql))
							{
								Logger?.LogError("Check your TableInfo attribute contains UpdateColumns value.");
								throw new SpiderException("UpdateSql is null.");
							}

							count += conn.MyExecute(sqlStatements.UpdateSql, items, transaction);
							break;
						}
						default:
						{
							count += conn.MyExecute(sqlStatements.InsertSql, items, transaction);
							break;
						}
					}

					if (transaction != null)
					{
						transaction.Commit();
						transaction.Dispose();
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

					throw;
				}
				finally
				{
					conn?.Dispose();
				}
			}

			throw new SpiderException("Pipeline process failed.");
		}

		protected string GetColumnName(Column column)
		{
			return IgnoreCase ? column.Name.ToLowerInvariant() : column.Name;
		}

		protected string GetDatabaseName(TableInfo tableInfo)
		{
			return IgnoreCase ? tableInfo.Schema.Database.ToLowerInvariant() : tableInfo.Schema.Database;
		}

		protected string GetTableName(TableInfo tableInfo)
		{
			return IgnoreCase ? tableInfo.Schema.FullName.ToLowerInvariant() : tableInfo.Schema.FullName;
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

			if (ConnectionStringSettingsRefresher == null)
				throw new SpiderException(
					"Can't find connection string from argument, config file, connectionStringSettingsRefresher.");
			{
				ConnectString = ConnectionStringSettingsRefresher.GetNew().ConnectionString;
				var conn = TryCreateDbConnection();
				if (conn != null)
				{
					return conn;
				}
			}

			throw new SpiderException(
				"Can't find connection string from argument, config file, connectionStringSettingsRefresher.");
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
			connectionStringSettings = connectionStringSettings ?? Env.DataConnectionStringSettings;
			if (connectionStringSettings == null)
			{
				return null;
			}

			IPipeline pipeline;
			switch (connectionStringSettings.ProviderName)
			{
				case DatabaseProviderFactories.PostgreSqlProvider:
				{
					pipeline = new PostgreSqlEntityPipeline(connectionStringSettings.ConnectionString);
					break;
				}
				case DatabaseProviderFactories.MySqlProvider:
				{
					pipeline = new MySqlEntityPipeline(connectionStringSettings.ConnectionString);
					break;
				}
				case DatabaseProviderFactories.SqlServerProvider:
				{
					pipeline = new SqlServerEntityPipeline(connectionStringSettings.ConnectionString);
					break;
				}

				case "MongoDB":
				{
#if !NET40
					pipeline = new MongoDbEntityPipeline(connectionStringSettings.ConnectionString);
					break;
#else
						throw new NotSupportedException("Notsupport mongodb in .NET40 right now.");
#endif
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
				Logger?.LogWarning($"Can't connect to database: {ConnectString}.");
			}

			return null;
		}
	}
}