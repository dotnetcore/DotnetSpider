using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Infrastructure;
using NLog;
using DotnetSpider.Core;
using System.Collections.Concurrent;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	public class CassandraEntityPipeline : BaseEntityPipeline
	{
		private PipelineMode _defaultPipelineModel;
		private static readonly TimeUuid DefaultTimeUuid = default(TimeUuid);
		private ConcurrentDictionary<string, EntityAdapter> EntityAdapters { get; set; } = new ConcurrentDictionary<string, EntityAdapter>();
		private ConcurrentDictionary<string, ISession> EntitySessions { get; set; } = new ConcurrentDictionary<string, ISession>();

		private CassandraConnectionSetting ConnectionSetting { get; set; }

		public CassandraEntityPipeline() : this(Env.DataConnectionStringSettings?.ConnectionString)
		{
		}

		public CassandraEntityPipeline(string connectString)
		{
			ConnectionSetting = new CassandraConnectionSetting(connectString);
		}

		public PipelineMode DefaultPipelineModel
		{
			get => _defaultPipelineModel;
			set
			{
				if (value == PipelineMode.Update)
				{
					throw new SpiderException("Can not set pipeline mode to Update.");
				}
				if (!Equals(value, _defaultPipelineModel))
				{
					_defaultPipelineModel = value;
				}
			}
		}

		public override void AddEntity(IEntityDefine entityDefine)
		{
			if (entityDefine == null)
			{
				throw new ArgumentException("Should not add a null entity to a entity dabase pipeline.");
			}

			if (!typeof(CassandraSpiderEntity).IsAssignableFrom(entityDefine.Type))
			{
				throw new ArgumentException("Cassandra pipeline only support CassandraSpiderEntity.");
			}

			if (entityDefine.TableInfo == null)
			{
				Logger.AllLog(Spider?.Identity, $"Schema is necessary, Skip {GetType().Name} for {entityDefine.Name}.", LogLevel.Warn);
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

		public override int Process(string name, List<dynamic> datas)
		{
			if (datas == null || datas.Count == 0)
			{
				return 0;
			}

			if (EntityAdapters.TryGetValue(name, out var metadata) && EntitySessions.TryGetValue(name, out var session))
			{
				switch (metadata.PipelineMode)
				{
					default:
					{
							var insertStmt = session.Prepare(metadata.InsertSql);
							var batch = new BatchStatement();
							foreach (var data in datas)
							{
								List<object> values = new List<object>();
								foreach (var column in metadata.Columns)
								{
									if (column.DataType.FullName != DataTypeNames.TimeUuid)
									{
										values.Add(column.Property.GetValue(data));
									}
									else
									{
										var value = column.Property.GetValue(data);
										values.Add(Equals(value, DefaultTimeUuid) ? TimeUuid.NewId() : value);
									}
								}

								batch.Add(insertStmt.Bind(values.ToArray()));
							}
							// Execute the batch
							session.Execute(batch);
							break;
						}
					case PipelineMode.InsertNewAndUpdateOld:
						{
							throw new NotImplementedException("Sql Server not suport InsertNewAndUpdateOld yet.");
						}
				}
			}
			return datas.Count;
		}

		private void InitDatabaseAndTable()
		{
			foreach (var adapter in EntityAdapters)
			{
				var cluster = CassandraUtils.CreateCluster(ConnectionSetting);
				var session = cluster.Connect();
				session.CreateKeyspaceIfNotExists(adapter.Value.Table.Database);
				session.ChangeKeyspace(adapter.Value.Table.Database);
				session.Execute(GenerateCreateTableSql(adapter.Value));
				var createIndexCql = GenerateCreateIndexes(adapter.Value);
				if (!string.IsNullOrEmpty(createIndexCql))
				{
					session.Execute(createIndexCql);
				}
				EntitySessions.AddOrUpdate(adapter.Key, session);
			}
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
				adapter.UpdateSql = GenerateUpdateSql();
			}
			adapter.SelectSql = GenerateSelectSql();
		}

		private string GenerateSelectSql()
		{
			return null;
		}

		private string GenerateUpdateSql()
		{
			return null;
		}

		private string GenerateInsertSql(EntityAdapter adapter)
		{
			var columNames = string.Join(", ", adapter.Columns.Select(p => $"{p.Name}"));
			var values = string.Join(", ", adapter.Columns.Select(column => $"?"));
			var tableName = adapter.Table.CalculateTableName();
			var sqlBuilder = new StringBuilder();

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
			builder.Append($", PRIMARY KEY(Id)");

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
					string indexColumNames = string.Join(", ", columns.Select(c => $"{c}"));

					builder.Append($"CREATE INDEX IF NOT EXISTS {name} ON {adapter.Table.Database}.{adapter.Table.CalculateTableName()}({indexColumNames});");
				}
			}
			if (adapter.Table.Uniques != null)
			{
				throw new SpiderException("Cassandra not support unique index.");
			}

			var sql = builder.ToString();
			return sql;
		}

		private string GetDataTypeSql(Column field)
		{
			var dataType = "text";

			if (field.DataType.FullName == DataTypeNames.Boolean)
			{
				dataType = "boolean";
			}
			else if (field.DataType.FullName == DataTypeNames.DateTime)
			{
				dataType = "timestamp";
			}
			else if (field.DataType.FullName == DataTypeNames.Decimal)
			{
				dataType = "decimal";
			}
			else if (field.DataType.FullName == DataTypeNames.Double)
			{
				dataType = "double";
			}
			else if (field.DataType.FullName == DataTypeNames.Float)
			{
				dataType = "float";
			}
			else if (field.DataType.FullName == DataTypeNames.Int)
			{
				dataType = "int";
			}
			else if (field.DataType.FullName == DataTypeNames.Int64)
			{
				dataType = "bigint";
			}
			else if (field.DataType.FullName == DataTypeNames.String)
			{
				dataType = "text";
			}
			else if (field.DataType.FullName == DataTypeNames.TimeUuid)
			{
				dataType = "uuid";
			}

			return dataType;
		}
	}
}
