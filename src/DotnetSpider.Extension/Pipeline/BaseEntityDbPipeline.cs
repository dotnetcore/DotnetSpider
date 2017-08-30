using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using DotnetSpider.Core;
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
		protected abstract string GenerateInsertSql(EntityAdapter adapter);
		protected abstract string GenerateUpdateSql(EntityAdapter adapter);
		protected abstract string GenerateSelectSql(EntityAdapter adapter);
		protected abstract string GenerateCreateTableSql(EntityAdapter adapter);
		protected abstract string GenerateCreateDatabaseSql(EntityAdapter adapter, string serverVersion);
		protected abstract string GenerateIfDatabaseExistsSql(EntityAdapter adapter, string serverVersion);
		protected abstract DbParameter CreateDbParameter(string name, object value);

		protected ConcurrentDictionary<string, EntityAdapter> EntityAdapters { get; set; } = new ConcurrentDictionary<string, EntityAdapter>();

		public IUpdateConnectString UpdateConnectString { get; set; }

		public ConnectionStringSettings ConnectionStringSettings { get; private set; }

		public bool CheckIfSameBeforeUpdate { get; set; }

		protected BaseEntityDbPipeline(string connectString = null, bool checkIfSaveBeforeUpdate = false)
		{
			ConnectionStringSettings = CreateConnectionStringSettings(connectString);
			CheckIfSameBeforeUpdate = checkIfSaveBeforeUpdate;
		}

		public override void AddEntity(EntityDefine entityDefine)
		{
			if (entityDefine == null)
			{
				throw new ArgumentException("Should not add a null entity to a entity dabase pipeline.");
			}

			if (entityDefine == null || entityDefine.Table == null)
			{
				Logger.MyLog(Spider?.Identity, $"Schema is necessary, Skip {GetType().Name} for {entityDefine.Name}.", LogLevel.Warn);
				return;
			}

			EntityAdapter entityAdapter = new EntityAdapter(entityDefine.Table, entityDefine.Columns);

			if (entityAdapter.Table.UpdateColumns != null && entityAdapter.Table.UpdateColumns.Length > 0)
			{
				entityAdapter.SelectSql = GenerateSelectSql(entityAdapter);
				entityAdapter.UpdateSql = GenerateUpdateSql(entityAdapter);
				entityAdapter.InsertModel = false;
			}

			entityAdapter.InsertSql = GenerateInsertSql(entityAdapter);
			EntityAdapters.TryAdd(entityDefine.Name, entityAdapter);
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


			foreach (var metadata in EntityAdapters.Values)
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

		public override void Process(string entityName, List<DataObject> datas)
		{
			EntityAdapter metadata;
			if (EntityAdapters.TryGetValue(entityName, out metadata))
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

								foreach (var column in metadata.Columns)
								{
									var value = data[$"{column.Name}"];
									var parameter = CreateDbParameter($"@{column.Name}", value);
									cmd.Parameters.Add(parameter);
								}

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
									if (string.IsNullOrEmpty(metadata.Table.Primary))
									{
										var primaryParameter = CreateDbParameter($"@{Core.Environment.IdColumn}", data[Core.Environment.IdColumn]);
										selectCmd.Parameters.Add(primaryParameter);
									}
									else
									{
										var columns = metadata.Table.Primary.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim());
										foreach (var column in columns)
										{
											var primaryParameter = CreateDbParameter($"@{column}", data[$"{column}"]);
											selectCmd.Parameters.Add(primaryParameter);
										}
									}

									var reader = selectCmd.ExecuteReader();
									DataObject old = new DataObject();
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
									if (old.Count == 0)
									{
										continue;
									}

									string oldValue = string.Join("-", old.Values);

									StringBuilder newValueBuilder = new StringBuilder();
									foreach (var column in metadata.Table.UpdateColumns)
									{
										var v = data[$"$.{column}"];
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

									foreach (var column in metadata.Table.UpdateColumns)
									{
										var parameter = CreateDbParameter($"@{column}", data[$"{column}"]);
										cmd.Parameters.Add(parameter);
									}

									if (string.IsNullOrEmpty(metadata.Table.Primary))
									{
										var primaryParameter = CreateDbParameter($"@{Core.Environment.IdColumn}", data[Core.Environment.IdColumn]);
										primaryParameter.DbType = DbType.String;
										cmd.Parameters.Add(primaryParameter);
									}
									else
									{
										var columns = metadata.Table.Primary.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim());
										foreach (var column in columns)
										{
											var primaryParameter = CreateDbParameter($"@{column}", data[$"{column}"]);
											cmd.Parameters.Add(primaryParameter);
										}
									}
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
			EntityAdapter metadata;
			if (EntityAdapters.TryGetValue(entityName, out metadata))
			{
				return metadata.Table.UpdateColumns;
			}
			return null;
		}
	}
}

