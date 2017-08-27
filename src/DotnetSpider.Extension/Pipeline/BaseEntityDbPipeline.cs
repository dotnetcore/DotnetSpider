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
using System.Collections.Concurrent;
using NLog;
using DotnetSpider.Core.Redial;
using DotnetSpider.Extension.Infrastructure;
using System.Configuration;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityDbPipeline : BaseEntityPipeline
	{
		protected abstract ConnectionStringSettings CreateConnectionStringSettings(string connectString = null);
		protected abstract string GenerateInsertSql(EntityDbMetadata metadata);
		protected abstract string GenerateUpdateSql(EntityDbMetadata metadata);
		protected abstract string GenerateSelectSql(EntityDbMetadata metadata);
		protected abstract string GenerateCreateTableSql(EntityDbMetadata metadata);
		protected abstract string GenerateCreateDatabaseSql(EntityDbMetadata metadata, string serverVersion);
		protected abstract string GenerateIfDatabaseExistsSql(EntityDbMetadata metadata, string serverVersion);
		protected abstract DbParameter CreateDbParameter(string name, object value);

		protected ConcurrentDictionary<string, EntityDbMetadata> DbMetadatas { get; set; } = new ConcurrentDictionary<string, EntityDbMetadata>();

		public IUpdateConnectString UpdateConnectString { get; set; }

		public ConnectionStringSettings ConnectionStringSettings { get; private set; }

		public bool CheckIfSameBeforeUpdate { get; set; }

		protected BaseEntityDbPipeline(string connectString = null, bool checkIfSaveBeforeUpdate = false)
		{
			ConnectionStringSettings = CreateConnectionStringSettings(connectString);
			CheckIfSameBeforeUpdate = checkIfSaveBeforeUpdate;
		}

		public override void AddEntity(Entity metadata)
		{
			if (metadata.Table == null)
			{
				Logger.MyLog(Spider?.Identity, $"Schema is necessary, Skip {GetType().Name} for {metadata.Name}.", LogLevel.Warn);
				return;
			}
			EntityDbMetadata dbMetadata = new EntityDbMetadata { Table = metadata.Table };
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
				throw new SpiderException($"Columns is necessary, Skip {GetType().Name} for {metadata.Name}.");
			}
			if (!string.IsNullOrEmpty(metadata.Table.Primary))
			{
				if (metadata.Table.Primary != Core.Environment.IdColumn)
				{
					var items = new HashSet<string>(metadata.Table.Primary.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()));
					if (items.Count > 0)
					{
						foreach (var item in items)
						{
							var column = dbMetadata.Columns.FirstOrDefault(c => c.Name == item);
							if (column == null)
							{
								throw new SpiderException("Columns set as Primary is not a property of your entity.");
							}
							if (column.Length > 256)
							{
								throw new SpiderException("Column length of Primary should not large than 256.");
							}
							column.NotNull = true;
						}
					}
					else
					{
						dbMetadata.Table.Primary = Core.Environment.IdColumn;
					}
				}
			}
			else
			{
				dbMetadata.Table.Primary = Core.Environment.IdColumn;
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

				dbMetadata.SelectSql = GenerateSelectSql(dbMetadata);
				dbMetadata.UpdateSql = GenerateUpdateSql(dbMetadata);

				dbMetadata.InsertModel = false;
			}

			if (dbMetadata.Table.Indexs != null && dbMetadata.Table.Indexs.Length > 0)
			{
				for (int i = 0; i < dbMetadata.Table.Indexs.Length; ++i)
				{
					var items = new HashSet<string>(dbMetadata.Table.Indexs[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()));

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
					var items = new HashSet<string>(dbMetadata.Table.Uniques[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()));

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

			dbMetadata.InsertSql = GenerateInsertSql(dbMetadata);
			DbMetadatas.TryAdd(metadata.Name, dbMetadata);
		}

		public override void InitPipeline(ISpider spider)
		{
			if (ConnectionStringSettings == null)
			{
				if (UpdateConnectString == null)
				{
					throw new SpiderException("ConnectionStringSettings or IUpdateConnectString are unfound.");
				}
				else
				{
					for (int i = 0; i < 5; ++i)
					{
						try
						{
							ConnectionStringSettings = UpdateConnectString.GetNew();
							break;
						}
						catch (Exception e)
						{
							Logger.MyLog(Spider.Identity, $"Update ConnectString failed.", LogLevel.Error, e);
							Thread.Sleep(1000);
						}
					}

					if (ConnectionStringSettings == null)
					{
						throw new SpiderException("Can not update ConnectionStringSettings via IUpdateConnectString.");
					}
				}
			}

			base.InitPipeline(spider);


			foreach (var metadata in DbMetadatas.Values)
			{
				if (!metadata.InsertModel)
				{
					continue;
				}

				NetworkCenter.Current.Execute("dbi", () =>
				{
					using (var conn = ConnectionStringSettings.GetDbConnection())
					{
						var command = conn.CreateCommand();
						command.CommandText = GenerateIfDatabaseExistsSql(metadata, conn.ServerVersion);

						if (Convert.ToInt16(command.ExecuteScalar()) == 0)
						{
							command.CommandText = GenerateCreateDatabaseSql(metadata, conn.ServerVersion);
							command.CommandType = CommandType.Text;
							command.ExecuteNonQuery();
						}

						command.CommandText = GenerateCreateTableSql(metadata);
						command.CommandType = CommandType.Text;
						command.ExecuteNonQuery();
					}
				});
			}
		}

		public override void Process(string entityName, List<JObject> datas)
		{
			EntityDbMetadata metadata;
			if (DbMetadatas.TryGetValue(entityName, out metadata))
			{
				NetworkCenter.Current.Execute("pp", () =>
				{
					if (metadata.InsertModel)
					{
						using (var conn = ConnectionStringSettings.GetDbConnection())
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
						}
					}
					else
					{
						using (var conn = ConnectionStringSettings.GetDbConnection())
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
										var primaryParameter = CreateDbParameter($"@{Core.Environment.IdColumn}", data.SelectToken(Core.Environment.IdColumn)?.Value<string>());
										primaryParameter.DbType = DbType.String;
										selectParameters.Add(primaryParameter);
									}
									else
									{
										var columns = metadata.Table.Primary.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim());
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
										var primaryParameter = CreateDbParameter($"@{Core.Environment.IdColumn}", data.SelectToken(Core.Environment.IdColumn)?.Value<string>());
										primaryParameter.DbType = DbType.String;
										parameters.Add(primaryParameter);
									}
									else
									{
										var columns = metadata.Table.Primary.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim());
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

