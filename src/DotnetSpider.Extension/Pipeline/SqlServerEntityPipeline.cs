using System;
using DotnetSpider.Extension.Infrastructure;
using System.Text;
using System.Linq;
using DotnetSpider.Extension.Model;
using System.Data;
using DotnetSpider.Extension.Model.Attribute;
using System.Data.Common;
using System.Data.SqlClient;
using Serilog;

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
		public SqlServerEntityPipeline(string connectString = null, PipelineMode pipelineMode = PipelineMode.InsertAndIgnoreDuplicate) : base(connectString, pipelineMode)
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
			var tableName = IgnoreColumnCase ? model.TableInfo.FullName.ToLower() : model.TableInfo.FullName;
			var database = IgnoreColumnCase ? model.TableInfo.Database.ToLower() : model.TableInfo.Database;

			var fields = model.Fields;
			var singleAutoIncrementPrimary = fields.Count(f => f.IsPrimary && (f.DataType == DataType.Int || f.DataType == DataType.Long)) == 1;

			StringBuilder builder = new StringBuilder($"USE {database}; IF OBJECT_ID('{tableName}', 'U') IS NULL CREATE table {tableName} (");

			foreach (var field in fields)
			{
				var columnSql = GenerateColumn(field);

				if (singleAutoIncrementPrimary && field.IsPrimary)
				{
					builder.Append($"{columnSql} IDENTITY(1,1), ");
				}
				else
				{
					builder.Append($"{columnSql}, ");
				}
			}
			builder.Remove(builder.Length - 2, 2);

			if (AutoTimestamp)
			{
				builder.Append($", creation_time DATETIME DEFAULT(GETDATE()), creation_date DATE DEFAULT(GETDATE())");
			}

			if (fields.Any(f => f.IsPrimary))
			{
				var primaryKeys = string.Join(", ", fields.Where(f => f.IsPrimary).Select(field => IgnoreColumnCase ? field.Name.ToLower() : field.Name));
				builder.Append(
					$", CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ({primaryKeys}) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = ON , ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY];");
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

		private string GenerateColumn(Field field)
		{
			var columnName = IgnoreColumnCase ? field.Name.ToLower() : field.Name;
			var dataType = GetDataTypeSql(field.DataType, field.Length);

			if (field.IsPrimary)
			{
				dataType = $"{dataType} NOT NULL";
			}

			return $"[{columnName}] {dataType}";
		}

		private string GenerateInsertSql(IModel model)
		{
			var fields = model.Fields;
			var singleAutoIncrementPrimary = fields.Count(f => f.IsPrimary && (f.DataType == DataType.Int || f.DataType == DataType.Long)) == 1;

			// 如果是单自增主键, 则不需要插入值
			var insertColumns = fields.Where(f => !f.IgnoreStore && (singleAutoIncrementPrimary ? !f.IsPrimary : true));

			string columnsSql = string.Join(", ", insertColumns.Select(p => $"[{(IgnoreColumnCase ? p.Name.ToLower() : p.Name)}]"));

			if (AutoTimestamp)
			{
				columnsSql = $"{columnsSql}, [creation_time], [creation_date]";
			}

			string columnsParamsSql = string.Join(", ", insertColumns.Select(p => $"@{p.Name}"));

			if (AutoTimestamp)
			{
				columnsParamsSql = $"{columnsParamsSql}, GETDATE(), GETDATE()";
			}

			var tableName = IgnoreColumnCase ? model.TableInfo.FullName.ToLower() : model.TableInfo.FullName;
			var database = IgnoreColumnCase ? model.TableInfo.Database.ToLower() : model.TableInfo.Database;

			var sql = $"USE {database}; INSERT INTO [{tableName}] ({columnsSql}) VALUES ({columnsParamsSql});";
			return sql;
		}

		private string GenerateUpdateSql(IModel model)
		{
			// 无主键, 无更新字段都无法生成更新SQL
			if (model.TableInfo.UpdateColumns == null || model.TableInfo.UpdateColumns.Count() == 0 || !model.Fields.Any(f => f.IsPrimary))
			{
				if (model.TableInfo.UpdateColumns == null || model.TableInfo.UpdateColumns.Count() == 0)
				{
					Log.Logger.Warning("Can't generate update sql, in table info, the count of update columns is zero.");
				}
				else
				{
					Log.Logger.Warning("Can't generate update sql, because in table info, because primary key is missing.");
				}
				return null;
			}

			var tableName = IgnoreColumnCase ? model.TableInfo.FullName.ToLower() : model.TableInfo.FullName;
			var database = IgnoreColumnCase ? model.TableInfo.Database.ToLower() : model.TableInfo.Database;

			var primaryKeys = model.Fields.Where(f => f.IsPrimary);
			string where = "";
			foreach (var field in model.Fields.Where(f => f.IsPrimary))
			{
				var primary = IgnoreColumnCase ? field.Name.ToLower() : field.Name;
				where += $" [{primary}] = @{field.Name} AND";
			}
			where = where.Substring(0, where.Length - 3);

			string setCols = string.Join(", ", model.TableInfo.UpdateColumns.Select(p => $"[{p.ToLower()}]=@{p}"));
			var sql = $"USE [{database}]; UPDATE [{tableName}] SET {setCols} WHERE {where};";
			return sql;
		}

		private string GenerateSelectSql(IModel model)
		{
			if (!model.Fields.Any(f => f.IsPrimary))
			{
				return null;
			}

			var tableName = IgnoreColumnCase ? model.TableInfo.FullName.ToLower() : model.TableInfo.FullName;
			var database = IgnoreColumnCase ? model.TableInfo.Database.ToLower() : model.TableInfo.Database;

			string where = "";
			foreach (var field in model.Fields.Where(f => f.IsPrimary))
			{
				var primary = IgnoreColumnCase ? field.Name.ToLower() : field.Name;
				where += $" [{primary}] = @{field.Name}";
			}

			var sql = $"USE [{database}] SELECT * FROM [{tableName}] WHERE {where};";
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
						dataType = "DATETIME DEFAULT(GETDATE())";
						break;
					}
				case DataType.Date:
					{
						dataType = "DATE DEFAULT(GETDATE())";
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
						dataType = length <= 0 || length >= 8000 ? "NVARCHAR(MAX)" : $"NVARCHAR({length})";
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
			var sqls = new Sqls();

			sqls.InsertSql = GenerateInsertSql(model);
			sqls.InsertAndIgnoreDuplicateSql = GenerateInsertSql(model);
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