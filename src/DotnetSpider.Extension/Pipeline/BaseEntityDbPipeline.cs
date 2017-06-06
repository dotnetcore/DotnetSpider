using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Model;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityDbPipeline : BaseEntityPipeline
	{
		public string ConnectString { get; set; }
		public bool CheckIfSameBeforeUpdate { get; set; }

		[JsonIgnore]
		public IUpdateConnectString UpdateConnectString { get; set; }
		protected abstract DbConnection CreateConnection();

		protected abstract string GetInsertSql(EntityDbMetadata metadata);
		protected abstract string GetUpdateSql(EntityDbMetadata metadata);
		protected abstract string GetSelectSql(EntityDbMetadata metadata);
		protected abstract string GetCreateTableSql(EntityDbMetadata metadata);

		protected abstract string GetCreateSchemaSql(EntityDbMetadata metadata, string serverVersion);
		protected abstract string GetIfSchemaExistsSql(EntityDbMetadata metadata, string serverVersion);
		protected abstract DbParameter CreateDbParameter(string name, object value);
		protected ConcurrentDictionary<string, EntityDbMetadata> DbMetadatas = new ConcurrentDictionary<string, EntityDbMetadata>();

		protected BaseEntityDbPipeline(string connectString, bool checkIfSaveBeforeUpdate = false)
		{
			ConnectString = connectString;
			CheckIfSameBeforeUpdate = checkIfSaveBeforeUpdate;
		}

		public override void AddEntity(Entity metadata)
		{
			if (metadata.Table == null)
			{
				Spider.Log($"Schema is necessary, Pass {GetType().Name} for {metadata.Name}.", LogLevel.Warn);
				return;
			}
			EntityDbMetadata dbMetadata = new EntityDbMetadata {Table = metadata.Table};
			foreach (var f in metadata.Fields)
			{
				var column = f;
				if (!column.IgnoreStore)
				{
					dbMetadata.Columns.Add(column);
				}
			}
			if (dbMetadata.Columns.Count == 0)
			{
				throw new SpiderException($"Columns is necessary, Pass {GetType().Name} for {metadata.Name}.");
			}
			if (!string.IsNullOrEmpty(metadata.Table.Primary))
			{
				var items = new HashSet<string>(metadata.Table.Primary.Split(','));
				if (items.Count > 0)
				{
					foreach (var item in items)
					{
						var column = dbMetadata.Columns.FirstOrDefault(c => c.Name == item);
						if (column == null)
						{
							throw new SpiderException("Columns set as Primary is not a property of your entity.");
						}
						if (column.Length <= 0 || column.Length > 256)
						{
							throw new SpiderException("Column length of Primary should not large than 256.");
						}
						column.NotNull = true;
					}
				}
				else
				{
					dbMetadata.Table.Primary = "__id";
				}
			}
			else
			{
				dbMetadata.Table.Primary = "__id";
			}

			if (dbMetadata.Table.UpdateColumns != null && dbMetadata.Table.UpdateColumns.Length > 0)
			{
				foreach (var column in dbMetadata.Table.UpdateColumns)
				{
					if (dbMetadata.Columns.All(c => c.Name != column))
					{
						throw new SpiderException("Columns set as update is not a property of your entity.");
					}
				}
				var updateColumns = new List<string>(dbMetadata.Table.UpdateColumns);
				updateColumns.Remove(dbMetadata.Table.Primary);

				dbMetadata.Table.UpdateColumns = updateColumns.ToArray();

				if (dbMetadata.Table.UpdateColumns.Length == 0)
				{
					throw new SpiderException("There is no column need update.");
				}

				dbMetadata.SelectSql = GetSelectSql(dbMetadata);
				dbMetadata.UpdateSql = GetUpdateSql(dbMetadata);

				dbMetadata.IsInsertModel = false;
			}

			if (dbMetadata.Table.Indexs != null && dbMetadata.Table.Indexs.Length > 0)
			{
				for (int i = 0; i < dbMetadata.Table.Indexs.Length; ++i)
				{
					var items = new HashSet<string>(dbMetadata.Table.Indexs[i].Split(','));

					if (items.Count == 0)
					{
						throw new SpiderException("Index should contain more than a column.");
					}
					foreach (var item in items)
					{
						var column = dbMetadata.Columns.FirstOrDefault(c => c.Name == item);
						if (column == null)
						{
							throw new SpiderException("Columns set as index is not a property of your entity.");
						}
						if (column.Length <= 0 || column.Length > 256)
						{
							throw new SpiderException("Column length of index should not large than 256.");
						}
					}
					dbMetadata.Table.Indexs[i] = string.Join(",", items);
				}
			}
			if (dbMetadata.Table.Uniques != null && dbMetadata.Table.Uniques.Length > 0)
			{
				for (int i = 0; i < dbMetadata.Table.Uniques.Length; ++i)
				{
					var items = new HashSet<string>(dbMetadata.Table.Uniques[i].Split(','));

					if (items.Count == 0)
					{
						throw new SpiderException("Unique should contain more than a column.");
					}
					foreach (var item in items)
					{
						var column = dbMetadata.Columns.FirstOrDefault(c => c.Name == item);
						if (column == null)
						{
							throw new SpiderException("Columns set as unique is not a property of your entity.");
						}
						if (column.DataType == DataType.Text && (column.Length <= 0 || column.Length > 256))
						{
							throw new SpiderException("Column length of unique should not large than 256.");
						}
					}
					dbMetadata.Table.Uniques[i] = string.Join(",", items);
				}
			}

			dbMetadata.InsertSql = GetInsertSql(dbMetadata);
			DbMetadatas.TryAdd(metadata.Name, dbMetadata);
		}

		public override void InitPipeline(ISpider spider)
		{
			if (string.IsNullOrEmpty(ConnectString))
			{
				if (UpdateConnectString == null)
				{
					throw new SpiderException("Can't find ConnectString or IUpdateConnectString.");
				}
				else
				{
					for (int i = 0; i < 5; ++i)
					{
						try
						{
							ConnectString = UpdateConnectString.GetNew();
							break;
						}
						catch (Exception e)
						{
							spider.Log("Update ConnectString failed.", LogLevel.Error, e);
							Thread.Sleep(1000);
						}
					}

					if (string.IsNullOrEmpty(ConnectString))
					{
						throw new SpiderException("Can't updadate ConnectString via IUpdateConnectString.");
					}
				}
			}

			base.InitPipeline(spider);


			foreach (var metadata in DbMetadatas.Values)
			{
				if (!metadata.IsInsertModel)
				{
					continue;
				}

				NetworkCenter.Current.Execute("db-init", () =>
				{
					using (DbConnection conn = CreateConnection())
					{
						var command = conn.CreateCommand();
						command.CommandText = GetIfSchemaExistsSql(metadata, conn.ServerVersion);

						if (Convert.ToInt16(command.ExecuteScalar()) == 0)
						{
							command.CommandText = GetCreateSchemaSql(metadata, conn.ServerVersion);
							command.CommandType = CommandType.Text;
							command.ExecuteNonQuery();
						}

						command.CommandText = GetCreateTableSql(metadata);
						command.CommandType = CommandType.Text;
						command.ExecuteNonQuery();
						conn.Close();
					}
				});
			}
		}

		public override void Process(string entityName, List<JObject> datas)
		{
			EntityDbMetadata metadata;
			if (DbMetadatas.TryGetValue(entityName, out metadata))
			{
				NetworkCenter.Current.Execute("pp-", () =>
				{
					if (metadata.IsInsertModel)
					{
						using (var conn = CreateConnection())
						{
							var cmd = conn.CreateCommand();
							cmd.CommandText = metadata.InsertSql;
							cmd.CommandType = CommandType.Text;

							foreach (var data in datas)
							{
								cmd.Parameters.Clear();

								List<DbParameter> parameters = new List<DbParameter>();
								foreach (var column in metadata.Columns)
								{
									var value = data.SelectToken($"{column.Name}")?.Value<string>();
									var parameter = CreateDbParameter($"@{column.Name}", value);
									parameter.DbType = DbType.String;
									parameters.Add(parameter);
								}

								cmd.Parameters.AddRange(parameters.ToArray());
								cmd.ExecuteNonQuery();
							}
							conn.Close();
						}
					}
					else
					{
						using (var conn = CreateConnection())
						{

							foreach (var data in datas)
							{
								bool needUpdate;
								if (CheckIfSameBeforeUpdate)
								{
									var selectCmd = conn.CreateCommand();
									selectCmd.CommandText = metadata.SelectSql;
									selectCmd.CommandType = CommandType.Text;
									List<DbParameter> selectParameters = new List<DbParameter>();
									if (string.IsNullOrEmpty(metadata.Table.Primary))
									{
										var primaryParameter = CreateDbParameter("@__Id", data.SelectToken("__Id")?.Value<string>());
										primaryParameter.DbType = DbType.String;
										selectParameters.Add(primaryParameter);
									}
									else
									{
										var columns = metadata.Table.Primary.Split(',');
										foreach (var column in columns)
										{
											var primaryParameter = CreateDbParameter($"@{column}", data.SelectToken($"{column}")?.Value<string>());
											primaryParameter.DbType = DbType.String;
											selectParameters.Add(primaryParameter);
										}
									}
									selectCmd.Parameters.AddRange(selectParameters.ToArray());
									var reader = selectCmd.ExecuteReader();
									JObject old = new JObject();
									if (reader.Read())
									{
										for (int i = 0; i < reader.FieldCount; ++i)
										{
											old.Add(reader.GetName(i), reader.GetString(i));
										}
									}
									reader.Dispose();
									selectCmd.Dispose();

									// the primary key is not exists.
									if (!old.HasValues)
									{
										continue;
									}

									string oldValue = string.Join("-", old.PropertyValues());

									StringBuilder newValueBuilder = new StringBuilder();
									foreach (var column in metadata.Table.UpdateColumns)
									{
										var v = data.SelectToken($"$.{column}");
										newValueBuilder.Append($"-{v}");
									}
									string newValue = newValueBuilder.ToString().Substring(1, newValueBuilder.Length - 1);
									needUpdate = oldValue != newValue;
								}
								else
								{
									needUpdate = true;
								}

								if (needUpdate)
								{
									var cmd = conn.CreateCommand();
									cmd.CommandText = metadata.UpdateSql;
									cmd.CommandType = CommandType.Text;

									List<DbParameter> parameters = new List<DbParameter>();
									foreach (var column in metadata.Table.UpdateColumns)
									{
										var parameter = CreateDbParameter($"@{column}", data.SelectToken($"{column}")?.Value<string>());
										parameter.DbType = DbType.String;
										parameters.Add(parameter);
									}

									if (string.IsNullOrEmpty(metadata.Table.Primary))
									{
										var primaryParameter = CreateDbParameter($"@__Id", data.SelectToken("__Id")?.Value<string>());
										primaryParameter.DbType = DbType.String;
										parameters.Add(primaryParameter);
									}
									else
									{
										var columns = metadata.Table.Primary.Split(',');
										foreach (var column in columns)
										{
											var primaryParameter = CreateDbParameter($"@{column}", data.SelectToken($"{column}")?.Value<string>());
											primaryParameter.DbType = DbType.String;
											parameters.Add(primaryParameter);
										}
									}
									//var primaryParameter = CreateDbParameter($"@{Schema.Primary}", data.SelectToken($"{Schema.Primary}")?.Value<string>());
									//primaryParameter.DbType = DbType.String;
									//parameters.Add(primaryParameter);

									cmd.Parameters.AddRange(parameters.ToArray());
									cmd.ExecuteNonQuery();
								}
							}

							conn.Close();
						}
					}
				});
			}
		}

		/// <summary>
		/// For test
		/// </summary>
		/// <returns></returns>
		public string[] GetUpdateColumns(string entityName)
		{
			EntityDbMetadata metadata;
			if (DbMetadatas.TryGetValue(entityName, out metadata))
			{
				return metadata.Table.UpdateColumns;
			}
			return null;
		}
	}
}

