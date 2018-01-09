using System.Linq;
using System.Text;
using DotnetSpider.Extension.Model;
using System.Configuration;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
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

		public override int Process(string entityName, IEnumerable<dynamic> datas, ISpider spider)
		{
			int count = 0;

			if (EntityAdapters.TryGetValue(entityName, out var metadata))
			{
				using (var conn = ConnectionStringSettings.CreateDbConnection())
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
								count += conn.MyExecute(metadata.InsertNewAndUpdateOldSql, datas);
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

		protected string GenerateCreateDatabaseSql(EntityAdapter adapter)
		{
			return $"CREATE SCHEMA IF NOT EXISTS `{adapter.Table.Database}` DEFAULT CHARACTER SET utf8mb4;";
		}

		protected string GenerateIfDatabaseExistsSql(EntityAdapter adapter)
		{
			return $"SELECT COUNT(*) FROM information_schema.SCHEMATA where SCHEMA_NAME='{adapter.Table.Database}';";
		}

		protected string GenerateCreateTableSql(EntityAdapter adapter)
		{
			var tableName = adapter.Table.CalculateTableName();
			StringBuilder builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS `{adapter.Table.Database }`.`{tableName}` (");
			string columNames = string.Join(", ", adapter.Columns.Select(GenerateColumn));
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
			builder.Append($", PRIMARY KEY (__Id)");
			builder.Append(") AUTO_INCREMENT=1");
			string sql = builder.ToString();
			return sql;
		}

		internal override void InitDatabaseAndTable()
		{
			foreach (var adapter in EntityAdapters.Values)
			{
				using (var conn = ConnectionStringSettings.CreateDbConnection())
				{
					var sql = GenerateIfDatabaseExistsSql(adapter);

					if (Convert.ToInt16(conn.MyExecuteScalar(sql)) == 0)
					{
						sql = GenerateCreateDatabaseSql(adapter);
						conn.MyExecute(sql);
					}

					sql = GenerateCreateTableSql(adapter);
					conn.MyExecute(sql);
				}
			}
		}

		private string GenerateInsertSql(EntityAdapter adapter, bool ignoreDuplicate)
		{
			var colNames = adapter.Columns.Where(p => !Env.IdColumns.Contains(p.Name) && p.Name != Env.CDateColumn).Select(p => p.Name).ToList();
			string cols = string.Join(", ", colNames.Select(p => $"`{p}`"));
			string colsParams = string.Join(", ", colNames.Select(p => $"@{p}"));
			var tableName = adapter.Table.CalculateTableName();

			var sql =
				$"INSERT {(ignoreDuplicate ? "IGNORE" : "")} INTO `{adapter.Table.Database}`.`{tableName}` ({cols}) VALUES ({colsParams});";
			return sql;
		}

		private string GenerateInsertNewAndUpdateOldSql(EntityAdapter adapter)
		{
			var colNames = adapter.Columns.Where(p => !Env.IdColumns.Contains(p.Name) && p.Name != Env.CDateColumn).Select(p => p.Name).ToList();
			string setParams = string.Join(", ", colNames.Select(p => $"`{p}`=@{p}"));
			string cols = string.Join(", ", colNames.Select(p => $"`{p}`"));
			string colsParams = string.Join(", ", colNames.Select(p => $"@{p}"));
			var tableName = adapter.Table.CalculateTableName();

			var sql =
				$"INSERT INTO `{adapter.Table.Database}`.`{tableName}` ({cols}) VALUES {colsParams} ON DUPLICATE KEY UPDATE {setParams};";

			return sql;
		}

		private string GenerateUpdateSql(EntityAdapter adapter)
		{
			string setParamenters = string.Join(", ", adapter.Table.UpdateColumns.Select(p => $"`{p}`=@{p}"));
			var tableName = adapter.Table.CalculateTableName();
			StringBuilder primaryParamenters = new StringBuilder();
			primaryParamenters.Append("`__Id` = @__Id");
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

			primaryParamenters.Append("`__Id` = @__Id");
			var tableName = adapter.Table.CalculateTableName();
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("SELECT * FROM `{0}`.`{1}` WHERE {2};",
				adapter.Table.Database,
				tableName,
				primaryParamenters);

			return sqlBuilder.ToString();
		}

		private string GetDataTypeSql(Column field)
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

		private string GenerateColumn(Column column)
		{
			if (column.DataType.FullName == DataTypeNames.DateTime)
			{
				return $"`{column.Name}` {GetDataTypeSql(column)} DEFAULT CURRENT_TIMESTAMP";
			}
			else if (Env.IdColumns.Contains(column.Name))
			{
				return "`__Id` bigint AUTO_INCREMENT";
			}
			else
			{
				return $"`{column.Name}` {GetDataTypeSql(column)}";
			}
		}
	}
}
