using System.Data.Common;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using DotnetSpider.Extension.Model;
using System.Configuration;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using System.IO;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure.Database;
using System;

namespace DotnetSpider.Extension.Pipeline
{
	public class MySqlEntityPipeline : BaseEntityDbPipeline
	{
		public MySqlEntityPipeline(string connectString = null) : base(connectString)
		{
			DefaultPipelineModel = PipelineMode.InsertAndIgnoreDuplicate;
		}

		protected override DbParameter CreateDbParameter(string name, object value)
		{
			var parameter = new MySqlParameter(name, MySqlDbType.String) { Value = value };
			return parameter;
		}

		protected string GetDataTypeSql(Column field)
		{
			var dataType = "LONGTEXT";

			if (field.DataType.FullName == DataTypeNames.Boolean)
			{
				dataType = "BOOL";
			}
			else if (field.DataType.FullName == DataTypeNames.DateTime)
			{
				dataType = "TIMESTAMP NULL";
			}
			else if (field.DataType.FullName == DataTypeNames.Decimal)
			{
				dataType = "DECIMAL(18,2)";
			}
			else if (field.DataType.FullName == DataTypeNames.Double)
			{
				dataType = "DOUBLE";
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
				dataType = (field.Length <= 0) ? "LONGTEXT" : $"VARCHAR({field.Length})";
			}

			return dataType;
		}

		protected override ConnectionStringSettings CreateConnectionStringSettings(string connectString = null)
		{
			ConnectionStringSettings connectionStringSettings;
			if (!string.IsNullOrEmpty(connectString))
			{
				connectionStringSettings = new ConnectionStringSettings("MySql", connectString, "MySql.Data.MySqlClient");
			}
			else
			{
				return null;
			}
			return connectionStringSettings;
		}

		public override int Process(string entityName, List<dynamic> datas)
		{
			if (datas == null || datas.Count == 0)
			{
				return 0;
			}
			int count = 0;

			if (EntityAdapters.TryGetValue(entityName, out var metadata))
			{
				using (var conn = ConnectionStringSettings.GetDbConnection())
				{
					switch (metadata.PipelineMode)
					{
						case PipelineMode.Insert:
							{
								count += conn.MyExecute(metadata.InsertSql, datas);
								break;
							}
						case PipelineMode.InsertAndIgnoreDuplicate:
							{
								count += conn.MyExecute(metadata.InsertAndIgnoreDuplicateSql, datas);
								break;
							}
						case PipelineMode.InsertNewAndUpdateOld:
							{
								count += conn.MyExecute(metadata.UpdateSql, datas);
								break;
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

		protected override void InitAllSqlOfEntity(EntityAdapter adapter)
		{
			adapter.InsertSql = GenerateInsertSql(adapter, false);
			adapter.InsertAndIgnoreDuplicateSql = GenerateInsertSql(adapter, true);
			if (adapter.PipelineMode == PipelineMode.Update)
			{
				adapter.UpdateSql = GenerateUpdateSql(adapter);
			}
			adapter.SelectSql = GenerateSelectSql(adapter);
			adapter.InsertNewAndUpdateOldSql = GenerateInsertNewAndUpdateOldSql(adapter);
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

		private string GenerateInsertSql(EntityAdapter adapter, bool ignoreDuplicate)
		{
			string columNames = string.Join(", ", adapter.Columns.Where(p => p.Name != Env.IdColumn && p.Name != Env.CDateColumn).Select(p => $"`{p.Name}`"));
			string values = string.Join(", ", adapter.Columns.Where(p => p.Name != Env.IdColumn && p.Name != Env.CDateColumn).Select(p => $"@{p.Name}"));
			var tableName = adapter.Table.CalculateTableName();
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("INSERT {0} INTO `{1}`.`{2}` {3} {4};",
				ignoreDuplicate ? "IGNORE" : "",
				adapter.Table.Database,
				tableName,
				string.IsNullOrEmpty(columNames) ? string.Empty : $"({columNames})",
				string.IsNullOrEmpty(values) ? string.Empty : $" VALUES ({values})");

			var sql = sqlBuilder.ToString();
			return sql;
		}

		private string GenerateInsertNewAndUpdateOldSql(EntityAdapter adapter)
		{
			string setParamenters = string.Join(", ", adapter.Columns.Where(p => p.Name != Env.IdColumn && p.Name != Env.CDateColumn).Select(p => $"`{p.Name}`=@{p.Name}"));
			string columNames = string.Join(", ", adapter.Columns.Where(p => p.Name != Env.IdColumn && p.Name != Env.CDateColumn).Select(p => $"`{p.Name}`"));
			string values = string.Join(", ", adapter.Columns.Where(p => p.Name != Env.IdColumn && p.Name != Env.CDateColumn).Select(p => $"@{p.Name}"));
			var tableName = adapter.Table.CalculateTableName();
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("INSERT INTO `{0}`.`{1}` {2} {3} ON DUPLICATE KEY UPDATE {4};",
				adapter.Table.Database,
				tableName,
				string.IsNullOrEmpty(columNames) ? string.Empty : $"({columNames})",
				string.IsNullOrEmpty(values) ? string.Empty : $" VALUES ({values})",
				setParamenters);

			var sql = sqlBuilder.ToString();
			return sql;
		}

		private string GenerateUpdateSql(EntityAdapter adapter)
		{
			string setParamenters = string.Join(", ", adapter.Table.UpdateColumns.Select(p => $"`{p}`=@{p}"));
			var tableName = adapter.Table.CalculateTableName();
			StringBuilder primaryParamenters = new StringBuilder();
			primaryParamenters.Append($"`{Env.IdColumn}` = @{Env.IdColumn}");
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("UPDATE `{0}`.`{1}` SET {2} WHERE {3};",
				adapter.Table.Database,
				tableName,
				setParamenters, primaryParamenters);

			return sqlBuilder.ToString();
		}

		private string GenerateSelectSql(EntityAdapter adapter)
		{
			StringBuilder primaryParamenters = new StringBuilder();

			primaryParamenters.Append($"`{Env.IdColumn}` = @{Env.IdColumn}");
			var tableName = adapter.Table.CalculateTableName();
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("SELECT * FROM `{0}`.`{1}` WHERE {2};",
				adapter.Table.Database,
				tableName,
				primaryParamenters);

			return sqlBuilder.ToString();
		}

		private string GenerateCreateTableSql(EntityAdapter adapter)
		{
			var tableName = adapter.Table.CalculateTableName();
			StringBuilder builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS `{adapter.Table.Database }`.`{tableName}` (");
			string columNames = string.Join(", ", adapter.Columns.Select(p => GenerateColumn(adapter, p)));
			builder.Append(columNames);

			if (adapter.Table.Indexs != null)
			{
				foreach (var index in adapter.Table.Indexs)
				{
					var columns = index.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string indexColumNames = string.Join(", ", columns.Select(c => $"`{c}`"));
					builder.Append($", KEY `index_{name}` ({indexColumNames.Substring(0, indexColumNames.Length)})");
				}
			}
			if (adapter.Table.Uniques != null)
			{
				foreach (var unique in adapter.Table.Uniques)
				{
					var columns = unique.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string uniqueColumNames = string.Join(", ", columns.Select(c => $"`{c}`"));
					builder.Append($", UNIQUE KEY `unique_{name}` ({uniqueColumNames.Substring(0, uniqueColumNames.Length)})");
				}
			}
			builder.Append($", PRIMARY KEY ({Env.IdColumn})");
			builder.Append(") AUTO_INCREMENT=1");
			string sql = builder.ToString();
			return sql;
		}

		private string GenerateColumn(EntityAdapter adapter, Column p)
		{
			if (p.DataType.FullName == DataTypeNames.DateTime)
			{
				return $"`{p.Name}` {GetDataTypeSql(p)} DEFAULT CURRENT_TIMESTAMP";
			}
			else if (Env.IdColumn == p.Name)
			{
				return $"`{Env.IdColumn}` bigint AUTO_INCREMENT";
			}
			else
			{
				return $"`{p.Name}` {GetDataTypeSql(p)}";
			}
		}

		private string GenerateCreateDatabaseSql(EntityAdapter adapter, string serverVersion)
		{
			return $"CREATE SCHEMA IF NOT EXISTS `{adapter.Table.Database}` DEFAULT CHARACTER SET utf8mb4;";
		}

		private string GenerateIfDatabaseExistsSql(EntityAdapter adapter, string serverVersion)
		{
			return $"SELECT COUNT(*) FROM information_schema.SCHEMATA where SCHEMA_NAME='{adapter.Table.Database}';";
		}
	}
}
