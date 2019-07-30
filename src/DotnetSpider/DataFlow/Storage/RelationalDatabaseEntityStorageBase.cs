using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.Common;
using DotnetSpider.DataFlow.Storage.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 关系型数据库保存实体解析结果
	/// </summary>
	public abstract class RelationalDatabaseEntityStorageBase : EntityStorageBase
	{
		private readonly Dictionary<string, SqlStatements> _sqlStatements = new Dictionary<string, SqlStatements>();

		protected const string BoolType = "Boolean";
		protected const string DateTimeType = "DateTime";
		protected const string DateTimeOffsetType = "DateTimeOffset";
		protected const string DecimalType = "Decimal";
		protected const string DoubleType = "Double";
		protected const string FloatType = "Single";
		protected const string IntType = "Int32";
		protected const string LongType = "Int64";
		protected const string ByteType = "Byte";
		protected const string ShortType = "Short";

		/// <summary>
		/// 创建数据库连接接口
		/// </summary>
		/// <param name="connectString">连接字符串</param>
		/// <returns></returns>
		protected abstract IDbConnection CreateDbConnection(string connectString);

		/// <summary>
		/// 生成 SQL 语句
		/// </summary>
		/// <param name="tableMetadata">表元数据</param>
		/// <returns></returns>
		protected abstract SqlStatements GenerateSqlStatements(TableMetadata tableMetadata);

		/// <summary>
		/// 创建数据库和表
		/// </summary>
		/// <param name="conn">数据库连接</param>
		/// <param name="sqlStatements">SQL 语句</param>
		protected virtual void EnsureDatabaseAndTableCreated(IDbConnection conn,
			SqlStatements sqlStatements)
		{
			if (!string.IsNullOrWhiteSpace(sqlStatements.CreateDatabaseSql))
			{
				conn.Execute(sqlStatements.CreateDatabaseSql);
			}

			conn.Execute(sqlStatements.CreateTableSql);
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="storageType">存储器类型</param>
		/// <param name="connectionString">连接字符串</param>
		protected RelationalDatabaseEntityStorageBase(StorageType storageType,
			string connectionString = null)
		{
			ConnectionString = connectionString;
			StorageType = storageType;
		}

		/// <summary>
		/// 存储器类型
		/// </summary>
		public StorageType StorageType { get; set; }

		/// <summary>
		/// 数据库操作重试次数
		/// </summary>
		public int RetryTimes { get; set; } = 600;

		/// <summary>
		/// 连接字符串
		/// </summary>
		public string ConnectionString { get; }

		/// <summary>
		/// 是否使用事务操作。默认不使用。
		/// </summary>
		public bool UseTransaction { get; set; }

		/// <summary>
		/// 数据库忽略大小写
		/// </summary>
		public bool IgnoreCase { get; set; } = true;

		protected override async Task<DataFlowResult> Store(DataFlowContext context)
		{
			IDbConnection conn = TryCreateDbConnection(context);

			using (conn)
			{
				foreach (var item in context.GetParseData())
				{
					var tableMetadata = (TableMetadata) context[item.Key];

					SqlStatements sqlStatements = GetSqlStatements(tableMetadata);

					// TODO: 需要优化，不能每次都需要尝试判断数据库
					lock (this)
					{
						EnsureDatabaseAndTableCreated(conn, sqlStatements);
					}

					for (int i = 0; i < RetryTimes; ++i)
					{
						IDbTransaction transaction = null;
						try
						{
							if (UseTransaction)
							{
								transaction = conn.BeginTransaction();
							}

							var list = item.Value;
							switch (StorageType)
							{
								case StorageType.Insert:
								{
									await conn.ExecuteAsync(sqlStatements.InsertSql, list, transaction);
									break;
								}

								case StorageType.InsertIgnoreDuplicate:
								{
									await conn.ExecuteAsync(sqlStatements.InsertIgnoreDuplicateSql, list, transaction);
									break;
								}

								case StorageType.Update:
								{
									if (string.IsNullOrWhiteSpace(sqlStatements.UpdateSql))
									{
										throw new SpiderException("未能生成更新 SQL");
									}

									await conn.ExecuteAsync(sqlStatements.UpdateSql, list, transaction);
									break;
								}

								case StorageType.InsertAndUpdate:
								{
									await conn.ExecuteAsync(sqlStatements.InsertAndUpdateSql, list, transaction);
									break;
								}
							}

							transaction?.Commit();
							break;
						}
						catch (Exception ex)
						{
							Logger?.LogError($"尝试插入数据失败: {ex}");

							// 网络异常需要重试，并且不需要 Rollback
							var endOfStreamException = ex.InnerException as EndOfStreamException;
							if (endOfStreamException == null)
							{
								try
								{
									transaction?.Rollback();
								}
								catch (Exception e)
								{
									Logger?.LogError($"数据库回滚失败: {e}");
								}

								break;
							}
						}
						finally
						{
							transaction?.Dispose();
						}
					}
				}
			}

			return DataFlowResult.Success;
		}

		protected virtual string GetNameSql(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			return IgnoreCase ? name.ToLowerInvariant() : name;
		}

		protected virtual string GetTableName(TableMetadata tableMetadata)
		{
			var tableName = tableMetadata.Schema.Table;
			switch (tableMetadata.Schema.TablePostfix)
			{
				case TablePostfix.Monday:
				{
					return $"{tableName}_{DateTimeHelper.MondayString}";
				}

				case TablePostfix.Month:
				{
					return $"{tableName}_{DateTimeHelper.MonthString}";
				}

				case TablePostfix.Today:
				{
					return $"{tableName}_{DateTimeHelper.TodayString}";
				}

				default:
				{
					return tableName;
				}
			}
		}

		private SqlStatements GetSqlStatements(TableMetadata tableMetadata)
		{
			// 每天执行一次建表操作, 可以实现每天一个表的操作，或者按周分表可以在运行时创建新表。
			var key = tableMetadata.TypeName;
			if (tableMetadata.Schema.TablePostfix != TablePostfix.None)
			{
				key = $"{key}-{DateTime.Now:yyyyMMdd}";
			}

			lock (this)
			{
				if (!_sqlStatements.ContainsKey(key))
				{
					_sqlStatements.Add(key, GenerateSqlStatements(tableMetadata));
				}

				return _sqlStatements[key];
			}
		}

		private IDbConnection TryCreateDbConnection(DataFlowContext context)
		{
			for (int i = 0; i < RetryTimes; ++i)
			{
				if (!string.IsNullOrWhiteSpace(ConnectionString))
				{
					var conn = TryCreateDbConnection(ConnectionString);
					if (conn != null)
					{
						return conn;
					}
				}

				var options = context.Services.GetRequiredService<SpiderOptions>();
				if (!string.IsNullOrWhiteSpace(options.StorageConnectionString))
				{
					var conn = TryCreateDbConnection(options.StorageConnectionString);
					if (conn != null)
					{
						return conn;
					}
				}

				Logger?.LogWarning("无有效的数据库连接配置");
			}

			throw new SpiderException(
				"无有效的数据库连接配置");
		}

		private IDbConnection TryCreateDbConnection(string connectionString)
		{
			try
			{
				var conn = CreateDbConnection(connectionString);
				conn.Open();
				return conn;
			}
			catch
			{
				Logger?.LogWarning($"无法打开数据库连接: {connectionString}.");
			}

			return null;
		}
	}
}