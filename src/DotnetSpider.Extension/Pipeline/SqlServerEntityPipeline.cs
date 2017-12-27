using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using DotnetSpider.Extension.Model;
using System.Configuration;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Extension.Pipeline
{
	public class SqlServerEntityPipeline : BaseEntityDbPipeline
	{
		public SqlServerEntityPipeline(string connectString = null) : base(connectString)
		{
			DefaultPipelineModel = PipelineMode.Insert;
		}

		protected virtual DbParameter CreateDbParameter(string name, object value)
		{
			if (value == null)
			{
				value = DBNull.Value;
			}
			return new SqlParameter(name, value);
		}

		private string GenerateCreateDatabaseSql(EntityAdapter adapter, string serverVersion)
		{
			string version = serverVersion.Split('.')[0];
			switch (version)
			{
				case "11":
					{
						return $"USE master; IF NOT EXISTS(SELECT * FROM sysdatabases WHERE name='{adapter.Table.Database}') CREATE DATABASE {adapter.Table.Database};";
					}
				default:
					{
						return $"USE master; IF NOT EXISTS(SELECT * FROM sys.databases WHERE name='{adapter.Table.Database}') CREATE DATABASE {adapter.Table.Database};";
					}
			}
		}

		private string GenerateIfDatabaseExistsSql(EntityAdapter adapter, string serverVersion)
		{
			string version = serverVersion.Split('.')[0];
			switch (version)
			{
				case "11":
					{
						return $"SELECT COUNT(*) FROM sysdatabases WHERE name='{adapter.Table.Database}'";
					}
				default:
					{
						return $"SELECT COUNT(*) FROM sys.databases WHERE name='{adapter.Table.Database}'";
					}
			}
		}

		private string GenerateCreateTableSql(EntityAdapter adapter)
		{
			var tableName = adapter.Table.CalculateTableName();
			StringBuilder builder = new StringBuilder($"USE {adapter.Table.Database}; IF OBJECT_ID('{tableName}', 'U') IS NULL CREATE table {tableName} (");
			StringBuilder columnNames = new StringBuilder();
			string columNames = string.Join(", ", adapter.Columns.Select(p => GenerateColumn(adapter, p)));
			builder.Append(columNames);
			builder.Append(",");
			builder.Append(
				$" CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED (__Id)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = {(adapter.PipelineMode == PipelineMode.InsertAndIgnoreDuplicate ? "ON" : "OFF")}, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY];");

			if (adapter.Table.Indexs != null)
			{
				foreach (var index in adapter.Table.Indexs)
				{
					var columns = index.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string indexColumNames = string.Join(", ", columns.Select(c => $"[{c}]"));
					builder.Append($"CREATE NONCLUSTERED INDEX [index_{name}] ON {tableName} ({indexColumNames.Substring(0, indexColumNames.Length)});");
				}
			}

			if (adapter.Table.Uniques != null)
			{
				foreach (var unique in adapter.Table.Uniques)
				{
					var columns = unique.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string uniqueColumNames = string.Join(", ", columns.Select(c => $"[{c}]"));
					builder.Append($"CREATE UNIQUE NONCLUSTERED INDEX [unique_{name}] ON {tableName} ({uniqueColumNames.Substring(0, uniqueColumNames.Length)}) {(adapter.PipelineMode == PipelineMode.InsertAndIgnoreDuplicate ? "WITH (IGNORE_DUP_KEY = ON)" : "") };");
				}
			}
			var sql = builder.ToString();
			return sql;
		}

		private string GenerateColumn(EntityAdapter adapter, Column p)
		{
			if (p.DataType.FullName == DataTypeNames.DateTime)
			{
				return $"[{p.Name}] {GetDataTypeSql(p)} DEFAULT(GETDATE())";
			}
			else if (Env.IdColumns.Contains(p.Name))
			{
				return $"[__Id] [bigint] IDENTITY(1,1) NOT NULL";
			}
			else
			{
				return $"[{p.Name}] {GetDataTypeSql(p)}";
			}
		}

		private string GenerateInsertSql(EntityAdapter adapter)
		{
			if (adapter.PipelineMode == PipelineMode.InsertAndIgnoreDuplicate)
			{
				throw new NotSupportedException("Sql Server not supported this model.");
			}

			var colNames = adapter.Columns.Where(p => !Env.IdColumns.Contains(p.Name) && p.Name != Env.CDateColumn).Select(p => p.Name);
			string cols = string.Join(", ", colNames.Select(p => $"[{p}]"));
			string colsParams = string.Join(", ", colNames.Select(p => $"@{p}"));
			var tableName = adapter.Table.CalculateTableName();

			var sql = string.Format("USE {0}; INSERT INTO [{1}] ({2}) VALUES ({3});",
				adapter.Table.Database,
				tableName,
				cols,
				colsParams);
			return sql;
		}

		private string GenerateUpdateSql(EntityAdapter adapter)
		{
			string setCols = string.Join(", ", adapter.Table.UpdateColumns.Select(p => $"[{p}]=@{p}"));

			var tableName = adapter.Table.CalculateTableName();
			var sql = string.Format("USE {0}; UPDATE [{1}] SET {2} WHERE [__Id] = @__Id;",
					adapter.Table.Database,
					tableName,
					setCols);

			return sql;
		}

		private string GenerateSelectSql(EntityAdapter adapter)
		{
			StringBuilder primaryParamenters = new StringBuilder();

			primaryParamenters.Append($"[__Id] = @__Id");

			var sqlBuilder = new StringBuilder();
			var tableName = adapter.Table.CalculateTableName();
			sqlBuilder.AppendFormat("USE {0}; SELECT * FROM [{1}] WHERE {2};",
				adapter.Table.Database,
				tableName,
				primaryParamenters.ToString());

			return sqlBuilder.ToString();
		}

		protected string GetDataTypeSql(Column field)
		{
			var dataType = "TEXT";

			if (field.DataType.FullName == DataTypeNames.Boolean)
			{
				dataType = "BOOL";
			}
			else if (field.DataType.FullName == DataTypeNames.DateTime)
			{
				dataType = "DATETIME";
			}
			else if (field.DataType.FullName == DataTypeNames.Decimal)
			{
				dataType = "DECIMAL(18,2)";
			}
			else if (field.DataType.FullName == DataTypeNames.Double)
			{
				dataType = "FLOAT";
			}
			else if (field.DataType.FullName == DataTypeNames.Float)
			{
				dataType = "FLOAT";
			}
			else if (field.DataType.FullName == DataTypeNames.Int)
			{
				dataType = "INT";
			}
			else if (field.DataType.FullName == DataTypeNames.Int64)
			{
				dataType = "BIGINT";
			}
			else if (field.DataType.FullName == DataTypeNames.String)
			{
				dataType = field.Length <= 0 ? "NVARCHAR(MAX)" : $"NVARCHAR({field.Length}) {(field.NotNull ? "NOT NULL" : "NULL")}";
			}

			return dataType;
		}

		protected override ConnectionStringSettings CreateConnectionStringSettings(string connectString = null)
		{
			ConnectionStringSettings connectionStringSettings;
			if (!string.IsNullOrEmpty(connectString))
			{
				connectionStringSettings = new ConnectionStringSettings("SqlServer", connectString, "System.Data.SqlClient");
			}
			else
			{
				if (Env.DataConnectionStringSettings != null)
				{
					connectionStringSettings = Env.DataConnectionStringSettings;
				}
				else
				{
					return null;
				}
			}
			return connectionStringSettings;
		}

		protected override void InitAllSqlOfEntity(EntityAdapter adapter)
		{
			if (adapter.PipelineMode == PipelineMode.InsertNewAndUpdateOld)
			{
				//Logger.MyLog(Spider.Identity, "Sql Server only check if primary key duplicate.", NLog.LogLevel.Warn);
				throw new NotImplementedException("Sql Server not suport InsertNewAndUpdateOld yet.");
			}
			adapter.InsertSql = GenerateInsertSql(adapter);
			adapter.SelectSql = GenerateSelectSql(adapter);
			adapter.InsertAndIgnoreDuplicateSql = GenerateInsertSql(adapter);
			if (adapter.PipelineMode == PipelineMode.Update)
			{
				adapter.UpdateSql = GenerateUpdateSql(adapter);
			}
		}

		internal override void InitDatabaseAndTable()
		{
			foreach (var adapter in EntityAdapters.Values)
			{
				using (var conn = ConnectionStringSettings.GetDbConnection())
				{
					var sql = GenerateIfDatabaseExistsSql(adapter, conn.ServerVersion);

					if (Convert.ToInt16(conn.MyExecuteScalar(sql)) == 0)
					{
						sql = GenerateCreateDatabaseSql(adapter, conn.ServerVersion);
						conn.MyExecute(sql);
					}

					sql = GenerateCreateTableSql(adapter);
					conn.MyExecute(sql);
				}
			}
		}

		public override int Process(string entityName, IEnumerable<dynamic> datas, ISpider spider)
		{
			int count = 0;
			if (EntityAdapters.TryGetValue(entityName, out var metadata))
			{
				using (var conn = ConnectionStringSettings.GetDbConnection())
				{
					switch (metadata.PipelineMode)
					{
						case PipelineMode.Insert:
						case PipelineMode.InsertAndIgnoreDuplicate:
							{
								count += conn.MyExecute(metadata.InsertSql, datas);
								break;
							}
						case PipelineMode.InsertNewAndUpdateOld:
							{
								throw new NotImplementedException("Sql Server not suport InsertNewAndUpdateOld yet.");
							}
						case PipelineMode.Update:
							{
								count += conn.MyExecute(metadata.UpdateSql, datas);
								break;
							}
						default:
							{
								count += conn.MyExecute(metadata.InsertSql, datas);
								break;
							}
					}
				}
			}
			return count;
		}
	}
}