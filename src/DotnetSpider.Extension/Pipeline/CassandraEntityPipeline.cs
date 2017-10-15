using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Infrastructure;
using NLog;
using DotnetSpider.Core;
using System.Collections.Concurrent;
using System.Configuration;
using Cassandra.Mapping;

namespace DotnetSpider.Extension.Pipeline
{
	public class Address
	{
		public string Street { get; set; }
		public string City { get; set; }
		public int ZipCode { get; set; }
		public IEnumerable<string> Phones { get; set; }
	}

	public class CassandraEntityPipeline : BaseEntityPipeline
	{
		private PipelineMode _defaultPipelineModel;

		internal ConcurrentDictionary<string, EntityAdapter> EntityAdapters { get; set; } = new ConcurrentDictionary<string, EntityAdapter>();
		internal ConcurrentDictionary<string, ISession> EntitySessions { get; set; } = new ConcurrentDictionary<string, ISession>();

		public string ConnectString { get; set; }

		public CassandraEntityPipeline()
		{
			ConnectString = Env.DataConnectionStringSettings?.ConnectionString;
		}

		public CassandraEntityPipeline(string connectString)
		{
			ConnectString = connectString;
		}

		public PipelineMode DefaultPipelineModel
		{
			get
			{
				return _defaultPipelineModel;
			}
			set
			{
				if (value == PipelineMode.Update)
				{
					throw new SpiderException("Can not set pipeline mode to Update.");
				}
				if (value != _defaultPipelineModel)
				{
					_defaultPipelineModel = value;
				}
			}
		}

		public override void AddEntity(EntityDefine entityDefine)
		{
			if (entityDefine == null)
			{
				throw new ArgumentException("Should not add a null entity to a entity dabase pipeline.");
			}

			if (entityDefine.TableInfo == null)
			{
				Logger.MyLog(Spider?.Identity, $"Schema is necessary, Skip {GetType().Name} for {entityDefine.Name}.", LogLevel.Warn);
				return;
			}

			EntityAdapter entityAdapter = new EntityAdapter(entityDefine.TableInfo, entityDefine.Columns);

			if (entityAdapter.Table.UpdateColumns != null && entityAdapter.Table.UpdateColumns.Length > 0)
			{
				entityAdapter.PipelineMode = PipelineMode.Update;
			}
			else
			{
				entityAdapter.PipelineMode = DefaultPipelineModel;
			}

			InitAllCqlOfEntity(entityAdapter);

			EntityAdapters.TryAdd(entityDefine.Name, entityAdapter);
		}

		public override void InitPipeline(ISpider spider)
		{
			base.InitPipeline(spider);

			InitDatabaseAndTable();
		}

		private void InitDatabaseAndTable()
		{
			foreach (var adapter in EntityAdapters)
			{
				var cluster = Cluster.Builder()
				.AddContactPoints(ConnectString)
				.Build();

				var session = cluster.Connect();
				EntitySessions.AddOrUpdate(adapter.Key, session);
				session.CreateKeyspaceIfNotExists(adapter.Value.Table.Database);
				session.ChangeKeyspace(adapter.Value.Table.Database);
				//session.Execute($"DROP table {adapter.Value.Table.Database}.{adapter.Value.Table.Name};");
				session.Execute(GenerateCreateTableSql(adapter.Value));
				//session.Execute(GenerateCreateIndexes(adapter.Value));

			}
		}

		public override int Process(string entityName, List<DataObject> datas)
		{
			if (datas == null || datas.Count == 0)
			{
				return 0;
			}
			int count = 0;
			if (EntityAdapters.TryGetValue(entityName, out var metadata) && EntitySessions.TryGetValue(entityName, out var session))
			{
				switch (metadata.PipelineMode)
				{
					case PipelineMode.Insert:
						{
							var insertStmt = session.Prepare(metadata.InsertSql);
							var batch = new BatchStatement();
							foreach (var data in datas)
							{
								data.Remove(Env.IdColumn);
								var now = DateTime.Now;
								data.Add("CDate", DateTime.Now);
								data["run_id"] = DateTime.Now;
								batch.Add(insertStmt.Bind(data.Values.ToArray()));
							};
							// Execute the batch
							session.Execute(batch);

							// ...you should reuse the prepared statement
							// Bind the parameters and add the statement to the batch batch
							break;
						}
					case PipelineMode.InsertAndIgnoreDuplicate:
						{
							IMapper mapper = new Mapper(session);
							foreach (var data in datas)
							{
								mapper.InsertIfNotExists<DataObject>(data);
							}
							break;
						}
					case PipelineMode.InsertNewAndUpdateOld:
						{
							throw new NotImplementedException("Sql Server not suport InsertNewAndUpdateOld yet.");
						}
					case PipelineMode.Update:
						{
							break;
						}
					default:
						{
							var insertStmt = session.Prepare(metadata.InsertSql);
							IMapper mapper = new Mapper(session);
							// ...you should reuse the prepared statement
							// Bind the parameters and add the statement to the batch batch
							foreach (var data in datas)
							{
								mapper.Insert<DataObject>(data);
							}
							break;
						}
				}
			}
			return count;
		}

		private void InitAllCqlOfEntity(EntityAdapter adapter)
		{
			if (adapter.PipelineMode == PipelineMode.InsertNewAndUpdateOld)
			{
				//Logger.MyLog(Spider.Identity, "Cassandra only check if primary key duplicate.", NLog.LogLevel.Warn);
				throw new NotImplementedException("Cassandra not suport InsertNewAndUpdateOld yet.");
			}
			adapter.InsertSql = GenerateInsertSql(adapter);
			if (adapter.PipelineMode == PipelineMode.Update)
			{
				adapter.UpdateSql = GenerateUpdateSql(adapter);
			}
			adapter.SelectSql = GenerateSelectSql(adapter);
		}

		private string GenerateSelectSql(EntityAdapter adapter)
		{
			return null;
		}

		private string GenerateUpdateSql(EntityAdapter adapter)
		{
			return null;
		}

		private string GenerateInsertSql(EntityAdapter adapter)
		{
			//INSERT INTO user_track (key, text, date) VALUES (?, ?, ?)
			var columNamesBuilder = new StringBuilder(string.Join(", ", adapter.Columns.Select(p => $"{p.Name}")));
			columNamesBuilder.Append(", CDate");
			if (Env.IdColumn.ToLower() == adapter.Table.Primary.ToLower())
			{
				columNamesBuilder.Append($", {Env.IdColumn}");
			}
			else
			{
				// 复合主键
			}

			var columNames = columNamesBuilder.ToString();

			var valuesBuilder = new StringBuilder();
			int valuesCount = 0;
			if (Env.IdColumn.ToLower() == adapter.Table.Primary.ToLower())
			{
				valuesCount = adapter.Columns.Count + 2;
			}
			else
			{
				// 复合主键
			}
			int lastIndex = valuesCount - 1;
			int cdateIndex = lastIndex - 1;
			for (int i = 0; i < valuesCount; ++i)
			{
				if (i == lastIndex)
				{
					valuesBuilder.Append("uuid()");
				}
				//else if (i == cdateIndex)
				//{
				//	valuesBuilder.Append("toTimestamp(now()),");
				//}
				else
				{
					valuesBuilder.Append("?,");
				}
			}
			var values = valuesBuilder.ToString();
			var tableName = adapter.Table.CalculateTableName();
			var sqlBuilder = new StringBuilder();
			//				adapter.Table.Database,
			sqlBuilder.AppendFormat("INSERT INTO {0} {1} {2};",
				tableName,
				string.IsNullOrEmpty(columNames) ? string.Empty : $"({columNames})",
				string.IsNullOrEmpty(values) ? string.Empty : $" VALUES ({values})");

			var sql = sqlBuilder.ToString();
			return sql;
		}

		private string GenerateCreateTableSql(EntityAdapter adapter)
		{
			var tableName = adapter.Table.CalculateTableName();

			StringBuilder builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {adapter.Table.Database }.{tableName} (");
			string columNames = string.Join(", ", adapter.Columns.Select(p => $"{p.Name} {GetDataTypeSql(p)} "));
			builder.Append(columNames);
			builder.Append(",CDate timestamp");
			if (Env.IdColumn.ToLower() == adapter.Table.Primary.ToLower())
			{
				builder.Append($", {Env.IdColumn} UUID PRIMARY KEY");
			}

			builder.Append(")");
			string sql = builder.ToString();
			return sql;
		}

		private string GenerateCreateIndexes(EntityAdapter adapter)
		{
			StringBuilder builder = new StringBuilder();
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

			//			CREATE INDEX IF NOT EXISTS index_name
			//ON keyspace_name.table_name(KEYS(column_name))
			return "";
		}
		private string GetDataTypeSql(Column field)
		{
			var dataType = "text";

			if (field.DataType == DataTypeNames.Boolean)
			{
				dataType = "boolean";
			}
			else if (field.DataType == DataTypeNames.DateTime)
			{
				dataType = "timestamp";
			}
			else if (field.DataType == DataTypeNames.Decimal)
			{
				dataType = "decimal";
			}
			else if (field.DataType == DataTypeNames.Double)
			{
				dataType = "double";
			}
			else if (field.DataType == DataTypeNames.Float)
			{
				dataType = "float";
			}
			else if (field.DataType == DataTypeNames.Int)
			{
				dataType = "int";
			}
			else if (field.DataType == DataTypeNames.Int64)
			{
				dataType = "bigint";
			}
			else if (field.DataType == DataTypeNames.String)
			{
				dataType = (field.Length <= 0) ? "text" : $"varchar({field.Length})";
			}

			return dataType;
		}
	}
}
