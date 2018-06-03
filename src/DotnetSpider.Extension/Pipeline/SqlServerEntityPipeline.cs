using System;
using DotnetSpider.Extension.Infrastructure;
using System.Text;
using System.Linq;
using DotnetSpider.Extension.Model;
using System.Data;
using DotnetSpider.Extension.Model.Attribute;
using System.Data.Common;
using System.Data.SqlClient;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到SqlServer中
	/// </summary>
	public class SqlServerEntityPipeline : DbModelPipeline
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">数据库连接字符串, 如果为空框架会尝试从配置文件中读取</param>
		/// <param name="pipelineMode">数据管道模式</param>
		public SqlServerEntityPipeline(string connectString = null, PipelineMode pipelineMode = PipelineMode.Insert) : base(connectString, pipelineMode)
		{
		}


		private string GenerateCreateDatabaseSql(IModel model, string serverVersion)
		{
			string version = serverVersion.Split('.')[0];
			switch (version)
			{
				case "11":
					{
						return $"USE master; IF NOT EXISTS(SELECT * FROM sysdatabases WHERE name='{model.TableInfo.Database}') CREATE DATABASE {model.TableInfo.Database};";
					}
				default:
					{
						return $"USE master; IF NOT EXISTS(SELECT * FROM sys.databases WHERE name='{model.TableInfo.Database}') CREATE DATABASE {model.TableInfo.Database};";
					}
			}
		}

		private string GenerateIfDatabaseExistsSql(IModel model, string serverVersion)
		{
			string version = serverVersion.Split('.')[0];
			switch (version)
			{
				case "11":
					{
						return $"SELECT COUNT(*) FROM sysdatabases WHERE name='{model.TableInfo.Database}'";
					}
				default:
					{
						return $"SELECT COUNT(*) FROM sys.databases WHERE name='{model.TableInfo.Database}'";
					}
			}
		}

		private string GenerateCreateTableSql(IModel model)
		{
			var tableName = model.TableInfo.FullName;
			var database = model.TableInfo.Database;

			StringBuilder builder = new StringBuilder($"USE {database}; IF OBJECT_ID('{tableName}', 'U') IS NULL CREATE table {tableName} (");
			StringBuilder columnNames = new StringBuilder();
			string columNames = string.Join(", ", model.Fields.Select(p => GenerateColumn(p.Name, p.DataType, p.Length)));
			builder.Append(columNames);

			if (AutoTimestamp)
			{
				builder.Append($", creation_time DATETIME, creation_date DATE");
			}

			if (!string.IsNullOrWhiteSpace(model.TableInfo.PrimaryKey))
			{
				var primaryKey = model.TableInfo.PrimaryKey.ToLower();
				builder.Append($", {primaryKey} BIGINT IDENTITY(1,1) NOT NULL");
				builder.Append(
					$" CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ({primaryKey}) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = {(PipelineMode == PipelineMode.InsertAndIgnoreDuplicate ? "ON" : "OFF")}, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY];");
			}
			else
			{
				builder.Append(") ON [PRIMARY];");
			}

			if (model.TableInfo.Indexs != null)
			{
				foreach (var index in model.TableInfo.Indexs)
				{
					var columns = index.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string indexColumNames = string.Join(", ", columns.Select(c => $"[{c.ToLower()}]"));
					builder.Append($"CREATE NONCLUSTERED INDEX [index_{name}] ON {tableName} ({indexColumNames.Substring(0, indexColumNames.Length)});");
				}
			}

			if (model.TableInfo.Uniques != null)
			{
				foreach (var unique in model.TableInfo.Uniques)
				{
					var columns = unique.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string uniqueColumNames = string.Join(", ", columns.Select(c => $"[{c.ToLower()}]"));
					builder.Append($"CREATE UNIQUE NONCLUSTERED INDEX [unique_{name}] ON {tableName} ({uniqueColumNames.Substring(0, uniqueColumNames.Length)}) {(PipelineMode == PipelineMode.InsertAndIgnoreDuplicate ? "WITH (IGNORE_DUP_KEY = ON)" : "") };");
				}
			}
			var sql = builder.ToString();
			return sql;
		}

		private string GenerateColumn(string name, DataType type, int length)
		{
			if (type == DataType.DateTime || type == DataType.Date)
			{
				return $"[{name.ToLower()}] {GetDataTypeSql(type, length)} DEFAULT(GETDATE())";
			}
			else
			{
				return $"[{name.ToLower()}] {GetDataTypeSql(type, length)}";
			}
		}

		private string GenerateInsertSql(IModel model)
		{
			var columns = model.Fields;
			var columnNames = columns.Select(p => p.Name);
			string columnsSql = string.Join(", ", columnNames.Select(p => $"[{p.ToLower()}]"));
			string columnsParameterSql = string.Join(", ", columnNames.Select(p => $"@{p}"));
			var tableName = model.TableInfo.FullName;

			var sql = string.Format("USE {0}; INSERT INTO [{1}] ({2}) VALUES ({3});",
				model.TableInfo.Database,
				tableName,
				columnsSql,
				columnsParameterSql);
			return sql;
		}

		private string GenerateUpdateSql(IModel model)
		{
			if (model.TableInfo.UpdateColumns == null || model.TableInfo.UpdateColumns.Count() == 0 || string.IsNullOrWhiteSpace(model.TableInfo.PrimaryKey))
			{
				return null;
			}
			var primaryKey = model.TableInfo.PrimaryKey.ToLower();
			string setColumnsSql = string.Join(", ", model.TableInfo.UpdateColumns.Select(p => $"[{p}] = @{p}"));
			var sql = $"USE {model.TableInfo.Database}; UPDATE [{model.TableInfo.FullName}] SET {setColumnsSql} WHERE [{primaryKey}] = @{primaryKey};";

			return sql;
		}

		private string GenerateSelectSql(IModel model)
		{
			if (string.IsNullOrWhiteSpace(model.TableInfo.PrimaryKey))
			{
				return null;
			}
			var primaryKey = model.TableInfo.PrimaryKey.ToLower();
			var sql = $"USE {model.TableInfo.Database}; SELECT * FROM [{ model.TableInfo.FullName}] WHERE [{primaryKey}] = @{primaryKey};";
			return sql;
		}

		private string GetDataTypeSql(DataType type, int length)
		{
			var dataType = "TEXT";

			switch (type)
			{
				case DataType.Bool:
					{
						dataType = "BIT";
						break;
					}
				case DataType.DateTime:
					{
						dataType = "DATETIME";
						break;
					}
				case DataType.Date:
					{
						dataType = "DATE";
						break;
					}
				case DataType.Decimal:
					{
						dataType = "DECIMAL(18,2)";
						break;
					}
				case DataType.Double:
					{
						dataType = "FLOAT";
						break;
					}
				case DataType.Float:
					{
						dataType = "FLOAT";
						break;
					}
				case DataType.Int:
					{
						dataType = "INT";
						break;
					}
				case DataType.Long:
					{
						dataType = "BIGINT";
						break;
					}
				default:
					{
						dataType = length <= 0 ? "NVARCHAR(MAX)" : $"NVARCHAR({length})";
						break;
					}
			}
			return dataType;
		}

		protected override IDbConnection CreateDbConnection(string connectString)
		{
			return new SqlConnection(connectString);
		}

		protected override Sqls GenerateSqls(IModel model)
		{
			if (PipelineMode == PipelineMode.InsertNewAndUpdateOld)
			{
				throw new NotImplementedException("Sql Server not suport InsertNewAndUpdateOld yet.");
			}
			if (PipelineMode == PipelineMode.InsertAndIgnoreDuplicate)
			{
				throw new NotImplementedException("Sql Server not suport InsertAndIgnoreDuplicate yet.");
			}

			var sqls = new Sqls();

			sqls.InsertSql = GenerateInsertSql(model);
			sqls.UpdateSql = GenerateUpdateSql(model);
			sqls.SelectSql = GenerateSelectSql(model);
			return sqls;
		}

		protected override void InitDatabaseAndTable(IDbConnection conn, IModel model)
		{
			var serverVersion = ((DbConnection)conn).ServerVersion;
			var sql = GenerateIfDatabaseExistsSql(model, serverVersion);

			if (Convert.ToInt16(conn.MyExecuteScalar(sql)) == 0)
			{
				sql = GenerateCreateDatabaseSql(model, serverVersion);
				conn.MyExecute(sql);
			}

			sql = GenerateCreateTableSql(model);
			conn.MyExecute(sql);
		}
	}
}