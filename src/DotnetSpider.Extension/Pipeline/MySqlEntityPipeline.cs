using System.Linq;
using System.Text;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Infrastructure;
using System.Data;
using MySql.Data.MySqlClient;
using DotnetSpider.Extension.Model.Attribute;
using Serilog;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到MySql中
	/// </summary>
	public class MySqlEntityPipeline : DbModelPipeline
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">数据库连接字符串, 如果为空框架会尝试从配置文件中读取</param>
		/// <param name="pipelineMode">数据管道模式</param>
		public MySqlEntityPipeline(string connectString = null, PipelineMode pipelineMode = PipelineMode.InsertAndIgnoreDuplicate) : base(connectString, pipelineMode)
		{
		}

		protected override IDbConnection CreateDbConnection(string connectString)
		{
			return new MySqlConnection(connectString);
		}

		protected override Sqls GenerateSqls(IModel model)
		{
			var sqls = new Sqls();
			sqls.InsertSql = GenerateInsertSql(model, false);
			sqls.InsertAndIgnoreDuplicateSql = GenerateInsertSql(model, true);
			sqls.InsertNewAndUpdateOldSql = GenerateInsertNewAndUpdateOldSql(model);
			sqls.UpdateSql = GenerateUpdateSql(model);
			sqls.SelectSql = GenerateSelectSql(model);
			return sqls;

		}

		protected override void InitDatabaseAndTable(IDbConnection conn, IModel model)
		{
			conn.MyExecute($"CREATE SCHEMA IF NOT EXISTS `{model.TableInfo.Database}` DEFAULT CHARACTER SET utf8mb4;");
			conn.MyExecute(GenerateCreateTableSql(model));
		}

		/// <summary>
		/// 构造创建数据表的SQL语句
		/// </summary>
		/// <param name="adapter">数据库管道使用的实体中间信息</param>
		/// <returns>SQL语句</returns>
		private string GenerateCreateTableSql(IModel model)
		{
			var tableName = model.TableInfo.FullName;
			var database = model.TableInfo.Database;
			var fields = model.Fields;

			StringBuilder builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS `{database}`.`{tableName}` (");
			string columNames = string.Join(", ", fields.Select(f => GenerateColumn(f.Name, f.DataType, f.Length)));
			if (AutoTimestamp)
			{
				columNames = $"{columNames}, `creation_time` TIMESTAMP DEFAULT CURRENT_TIMESTAMP, `creation_date` DATE";
			}
			if (!string.IsNullOrWhiteSpace(model.TableInfo.PrimaryKey))
			{
				columNames = $"{columNames}, {model.TableInfo.PrimaryKey.ToLower()} bigint AUTO_INCREMENT PRIMARY KEY";
			}

			builder.Append(columNames);

			if (model.TableInfo.Indexs != null)
			{
				foreach (var index in model.TableInfo.Indexs)
				{
					var columns = index.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string indexColumNames = string.Join(", ", columns.Select(c => $"`{c}`"));
					builder.Append($", KEY `index_{name}` ({indexColumNames.Substring(0, indexColumNames.Length)})");
				}
			}
			if (model.TableInfo.Uniques != null)
			{
				foreach (var unique in model.TableInfo.Uniques)
				{
					var columns = unique.Split(',');
					string name = string.Join("_", columns.Select(c => c));
					string uniqueColumNames = string.Join(", ", columns.Select(c => $"`{c}`"));
					builder.Append($", UNIQUE KEY `unique_{name}` ({uniqueColumNames.Substring(0, uniqueColumNames.Length)})");
				}
			}
			if (!string.IsNullOrWhiteSpace(model.TableInfo.PrimaryKey))
			{
				builder.Append(") AUTO_INCREMENT=1");
			}
			else
			{
				builder.Append(")");
			}
			string sql = builder.ToString();
			return sql;
		}

		private string GenerateInsertSql(IModel model, bool ignoreDuplicate)
		{
			var columns = model.Fields;
			var columnNames = columns.Select(p => p.Name.ToLower()).ToList();
			string cols = string.Join(", ", columnNames.Select(p => $"`{p.ToLower()}`"));
			if (AutoTimestamp)
			{
				cols = $"{cols}, `creation_date`";
			}
			string colsParams = string.Join(", ", columnNames.Select(p => $"@{p}"));
			if (AutoTimestamp)
			{
				colsParams = $"{colsParams}, CURRENT_DATE()";
			}
			var tableName = model.TableInfo.FullName;

			var sql =
				$"INSERT {(ignoreDuplicate ? "IGNORE" : "")} INTO `{model.TableInfo.Database}`.`{tableName}` ({cols}) VALUES ({colsParams});";
			return sql;
		}

		private string GenerateInsertNewAndUpdateOldSql(IModel model)
		{
			var columns = model.Fields;
			var columnNames = columns.Select(p => p.Name).ToList();
			string setParams = string.Join(", ", columnNames.Select(p => $"`{p.ToLower()}`=@{p}"));
			string cols = string.Join(", ", columnNames.Select(p => $"`{p.ToLower()}`"));
			string colsParams = string.Join(", ", columnNames.Select(p => $"@{p}"));
			var tableName = model.TableInfo.FullName;

			var sql =
				$"INSERT INTO `{model.TableInfo.Database}`.`{tableName}` ({cols}) VALUES ({colsParams}) ON DUPLICATE KEY UPDATE {setParams};";

			return sql;
		}

		private string GenerateUpdateSql(IModel model)
		{
			if (model.TableInfo.UpdateColumns == null || model.TableInfo.UpdateColumns.Count() == 0 || string.IsNullOrWhiteSpace(model.TableInfo.PrimaryKey))
			{
				if (model.TableInfo.UpdateColumns == null || model.TableInfo.UpdateColumns.Count() == 0)
				{
					Log.Logger.Warning("Can't generate update sql, in table info, the count of update columns is zero.");
				}
				else
				{
					Log.Logger.Warning("Can't generate update sql, because in table info, the primary key is null.");
				}
				return null;
			}
			var primaryKey = model.TableInfo.PrimaryKey.ToLower();
			string setCols = string.Join(", ", model.TableInfo.UpdateColumns.Select(p => $"`{p.ToLower()}`=@{p}"));
			var sql = $"UPDATE `{model.TableInfo.Database}`.`{model.TableInfo.FullName}` SET {setCols} WHERE `{primaryKey}` = @{primaryKey};";
			return sql;
		}

		private string GenerateSelectSql(IModel model)
		{
			if (string.IsNullOrWhiteSpace(model.TableInfo.PrimaryKey))
			{
				return null;
			}
			var primaryKey = model.TableInfo.PrimaryKey.ToLower();
			StringBuilder primaryParamenters = new StringBuilder();

			primaryParamenters.Append($"`{primaryKey}` = @{primaryKey}");
			var tableName = model.TableInfo.FullName;
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendFormat("SELECT * FROM `{0}`.`{1}` WHERE {2};",
				model.TableInfo.Database,
				tableName,
				primaryParamenters);

			return sqlBuilder.ToString();
		}

		private string GenerateColumn(string name, DataType type, int length)
		{
			return $"`{name.ToLower()}` {GetDataTypeSql(type, length)}";
		}

		private string GetDataTypeSql(DataType type, int length)
		{
			var dataType = "LONGTEXT";

			switch (type)
			{
				case DataType.Bool:
					{
						dataType = "BOOL";
						break;
					}
				case DataType.DateTime:
					{
						dataType = "TIMESTAMP DEFAULT CURRENT_TIMESTAMP";
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
						dataType = "DOUBLE";
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
						dataType = (length <= 0) ? "LONGTEXT" : $"VARCHAR({length})";
						break;
					}
			}
			return dataType;
		}
	}
}
